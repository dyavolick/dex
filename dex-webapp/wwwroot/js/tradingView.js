const dataItemRange = {
  Year: 1, Month: 2, Day: 3, Hour4: 4, Hour: 5, Minutes30: 6, Minutes15: 7, Minutes5: 8, Minutes3: 9, Minute: 10
}

const connection = new signalR.HubConnectionBuilder()
  .withUrl("/sr/chart")
  .configureLogging(signalR.LogLevel.Information)
  .build();

let subscribeHubName = null;
let subscribeBarsCallback = null;

let lastInsertedDate = 0;

let datafeed = {
  /* mandatory methods for realtime chart */

  onReady: cb => {
    console.log("datafeed onReady()");
    setTimeout(() => cb({}), 0);
  },

  // only need searchSymbols when search is enabled
  searchSymbols: (userInput, exchange, symbolType, onResultReadyCallback) => {
    console.log("datafeed searchSymbols()");
  },

  resolveSymbol: (symbolName, onSymbolResolvedCallback, onResolveErrorCallback) => {
    console.log("datafeed resolveSymbol()", symbolName);
    var symbol_stub = {
      name: symbolName,
      description: '',
      type: 'crypto',
      session: '24x7',
      timezone: 'GMT',
      ticker: symbolName,
      minmov: 1,
      pricescale: 100000000,
      has_intraday: true,
      intraday_multipliers: ['1', '60'],
      interval: 'D',
      volume_precision: 8,
      //data_status: 'streaming',
    };
    setTimeout(function () {
      onSymbolResolvedCallback(symbol_stub);
    }, 0);
  },

  getBars: (symbolInfo, resolution, from, to, onHistoryCallback, onErrorCallback, firstDataRequest) => {
    console.log("datafeed getBars()", symbolInfo, resolution, from, to, onHistoryCallback, onErrorCallback, firstDataRequest);

    getData(resolution, symbolInfo.name, from, to)
      .then((data) => {
        console.log("data", data);
        let bars;
        window.data = data;
        if (data.length) {
          bars = data.map(el => convertBarToChartObj(el));
        } else {
          bars = [];
        }
        let resultBars = addEmptyBars(symbolInfo.name, bars.slice(0), resolution, from, to);
        //console.log("filled bars", resultBars);
        onHistoryCallback(resultBars, { noData: !resultBars });
        lastInsertedDate = resultBars[resultBars.length - 1].time;
      })
      .catch(console.error);
  },

  subscribeBars: (symbolInfo, resolution, onRealtimeCallback, subscribeUID, onResetCacheNeededCallback) => {
    console.log("datafeed subscribeBars()", symbolInfo, resolution, subscribeUID);

    let newSubscribeHubName = `chartUpdate_${symbolInfo.name}_${convertTimeResolutionToServer(resolution)}`;
    console.log("subscribeHubName", subscribeHubName, newSubscribeHubName);
    //Common.hubConnection
    //  .invoke('ChartSubscribe', { GroupName: newSubscribeHubName, PrevGroupName: subscribeHubName })
    //  .catch(err => window.notify({ message: err, title: "ChartSubscribe Error", level: 'error' }));
    connection.invoke('ChartSubscribe', { GroupName: newSubscribeHubName, PrevGroupName: subscribeHubName })
      .catch(err => console.log(err));
    subscribeHubName = newSubscribeHubName;
    subscribeBarsCallback = onRealtimeCallback;
  },

  unsubscribeBars: subscriberUID => { console.log("datafeed unsubscribeBars()", subscriberUID); },


  /* optional methods */
  getServerTime: cb => { console.log("datafeed getServerTime()"); },
  calculateHistoryDepth: (resolution, resolutionBack, intervalBack) => { console.log("datafeed calculateHistoryDepth()"); },
  getMarks: (symbolInfo, startDate, endDate, onDataCallback, resolution) => { console.log("datafeed getMarks()"); },
  getTimeScaleMarks: (symbolInfo, startDate, endDate, onDataCallback, resolution) => { console.log("datafeed getTimeScaleMarks()"); }
};

function applyBarUpdate(lastBar) {
  console.log('applyBarUpdate');
  if (!subscribeBarsCallback)
    return;

  //console.log("lastBar", lastBar);
  let result = null;
  if (lastBar)
    result = convertBarToChartObj(lastBar);

  if (result) {
    console.log("update chart", result);
    if (result.time >= lastInsertedDate) {
      subscribeBarsCallback(result);
    } else {
      console.log("update is earlier than last inserted candle");
    }
  }
}

function getParameterByName(name) {
  name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
  var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
    results = regex.exec(location.search);
  return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}

function convertTimeResolutionToServer(chartResolution) {
  switch (chartResolution) {
    case "1": return dataItemRange.Minute;
    case "3": return dataItemRange.Minutes3;
    case "5": return dataItemRange.Minutes5;
    case "15": return dataItemRange.Minutes15;
    case "30": return dataItemRange.Minutes30;
    case "60": return dataItemRange.Hour;
    case "240": return dataItemRange.Hour4;
    case "D": return dataItemRange.Day;
    case "M": return dataItemRange.Month;
    case "12M": return dataItemRange.Year;
  }
}
function convertTimeResolutionToMs(chartResolution) {
  switch (chartResolution) {
    case "1": return 1000 * 60;
    case "3": return 3 * 1000 * 60;
    case "5": return 5 * 1000 * 60;
    case "15": return 15 * 1000 * 60;
    case "30": return 30 * 1000 * 60;
    case "60": return 1000 * 60 * 60;
    case "240": return 4 * 1000 * 60 * 60;
    case "D": return 1000 * 60 * 60 * 24;
    case "M": return 1000 * 60 * 60 * 24 * 30;
    case "12M": return 1000 * 60 * 60 * 24 * 365;
  }
}
function convertDateToServer(date) {
  return new Date(date * 1000).toISOString();
}
function convertBarToChartObj(el) {
  let date = new Date(el.date);
  return {
    time: Date.parse(date) - date.getTimezoneOffset() * 60 * 1000, //TradingView requires bar time in ms
    low: el.min,
    high: el.max,
    open: el.open,
    close: el.close,
    volume: el.volume
  };
}

function addEmptyBars(pairName, data, resolution, from, to) {
  if (convertTimeResolutionToServer(resolution) === dataItemRange.Day) {
    data = clearTimePart(data, resolution);
    //console.log("test",data); return;
  }
  //console.log("addEmptyBars()", data);
  let candlePeriod = convertTimeResolutionToMs(resolution);

  from *= 1000;
  to *= 1000;

  // if data is empty - get last price before the date range and create candles with this price
  if (!data || data.length === 0) {
    data = [];
    let lastPrice = getLastPrice(pairName, resolution, from);
    let emptyCandlesToInsert = Math.floor((to - from) / candlePeriod) - 1;
    //console.log("addEmptyBars() data is empty, loaded last price:", lastPrice, "inserting empty candles:", emptyCandlesToInsert);
    for (var j = 0; j < emptyCandlesToInsert; j++) {
      data.push(makeBarNoChanges(lastPrice, from + j * candlePeriod));
    }
    return data;
  }

  // otherwise add empty candles before, between and after the existing candles
  let emptyCandlesToInsertAtStart = Math.floor((data[0].time - from) / candlePeriod) - 2;
  let initalPrice = data[0].open;
  for (let j = 0; j < emptyCandlesToInsertAtStart; j++) {
    data.splice(j, 0, makeBarNoChanges(initalPrice, from + j * candlePeriod));
  }
  //console.log("emptyCandlesToInsertAtStart", emptyCandlesToInsertAtStart);

  for (let i = 1; i < data.length - 1; i++) {
    if (data[i + 1].time > data[i].time + candlePeriod) {
      let emptyCandlesToInsert = Math.floor((data[i + 1].time - data[i].time) / candlePeriod) - 1;
      for (let j = 0; j < emptyCandlesToInsert; j++) {
        data.splice(i + j + 1, 0, makeBarNoChanges(data[i].close, data[i].time + (j + 1) * candlePeriod));
      }
      i += emptyCandlesToInsert;
    }
  }

  let lastBar = data[data.length - 1];
  let emptyCandlesToInsertAtEnd = Math.floor((to - lastBar.time) / candlePeriod) - 1;
  for (let j = 0; j < emptyCandlesToInsertAtEnd; j++) {
    let date = lastBar.time + (j + 1) * candlePeriod;
    if (date + candlePeriod < to)
      data.push(makeBarNoChanges(lastBar.close, date));
  }
  //console.log("emptyCandlesToInsertAtEnd", emptyCandlesToInsertAtEnd);

  return data;
}
function makeBarNoChanges(price, time) {
  return {
    time: time,
    low: price, high: price, open: price, close: price,
    volume: 0, volumeBase: 0,
    isFake: true
  }
}

function clearTimePart(data, resolution) {
  let candlePeriod = convertTimeResolutionToMs(resolution);
  //console.log("clearTimePart()", candlePeriod);
  for (var i = 0; i < data.length; i++) {
    data[i].time = Math.floor(data[i].time / candlePeriod) * candlePeriod;
  }
  return data;
}

function getData(resolution, pair, from, to) {
  let serverResolution = convertTimeResolutionToServer(resolution);
  const url = `/api/trades/GetOHLC/${serverResolution}/${pair}/${convertDateToServer(from)}/${convertDateToServer(to)}`;
  return $.getJSON(url);
  //return Api.get(url);
}
function getLastBar(resolution, pair, before) {
  let serverResolution = convertTimeResolutionToServer(resolution);
  let url;
  if (before) {
    url = `/api/trades/ohlc-last-candle/${serverResolution}/${pair}/${convertDateToServer(before)}`;
  } else {
    url = `/api/trades/ohlc-last-candle/${serverResolution}/${pair}`;
  }
  return $.getJSON(url);

  //  return Api.get(url);
}

function getLastPrice(pair, resolution, before) {
  //console.log(`getLastPrice() resolution:${resolution}, pair:${pair}, before:${before}`);
  getLastBar(resolution, pair, before)
    .then(lastBar => {
      if (lastBar)
        return lastBar.close;
      else
        return 0;
    })
    .catch(err => {
      console.log("getLastPrice()", err);
      return 0;
    });
}

function initChart() {

  console.log('init chart');

  TradingView.onready(function () {
    connection.start().then(() => {
      console.log('connected to hub');

      connection.invoke('ChartSubscribe', { GroupName: subscribeHubName, PrevGroupName: subscribeHubName })
        .catch(err => console.log(err));
      var widget = window.tvWidget = new TradingView.widget({
        autosize: true,
        symbol: window.tokenSymbol, //Common.currencyPairId(),
        interval: 'D',
        timezone: "Etc/UTC",
        theme: "Light",
        style: "1",
        locale: "en",
        toolbar_bg: "#f1f3f6",
        enable_publishing: false,
        debug: true, // show Library errors and warnings in the console
        //fullscreen: false,
        //time_frame: 'M',
        save_image: false,
        hideideas: true,
        container_id: "tradingview_35f2b",
        //	BEWARE: no trailing slash is expected in feed URL
        datafeed: datafeed, //new Datafeeds.UDFCompatibleDatafeed("https://demo_feed.tradingview.com"),
        library_path: "/charting_library/",
        //supported_resolutions: ["1", "3", "5", "15", "30", "60", "240", "D", "M", "12M"],
        /*time_frames: [
          //{ text: "h", resolution: "60", description: "Hour" },
          //{ text: "d", resolution: "1D", description: "Day", title: "Day" },
          //{ text: "7d", resolution: "7D", description: "7 Days" },
          { text: "m", resolution: "1M", description: "1 Month" },
        ],*/
        //["60", "D", "7D", "M", "6M", "12M"],
        //	Regression Trend-related functionality is not implemented yet, so it's hidden for a while
        //drawings_access: { type: 'black', tools: [{ name: "Regression Trend" }] },
        disabled_features: ["use_localstorage_for_settings"],
        //enabled_features: ["study_templates"],
        //charts_storage_url: 'http://saveload.tradingview.com',
        //charts_storage_api_version: "1.1",
        client_id: 'tradingview.com',
        user_id: 'public_user_id'
      });
    }).catch(err => console.error(err.toString()));
  });

}

initChart();

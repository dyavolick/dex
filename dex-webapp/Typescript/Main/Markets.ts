/// <reference path="../../node_modules/@types/toastr/index.d.ts" />

export class Markets {

  constructor() {
    this.init();
  }

  async init() {
    $(".js-markets").on("change", "input.token-favorite", () => {
      var favoriteTokens = [];
      $("input.token-favorite:checked").each(function () {
        favoriteTokens.push($(this).closest("tr").attr("id").split("_")[1]);
      });
      (<any>window).Cookies.set('token-favorites', JSON.stringify(favoriteTokens));

      this.sortMarkets();
    });

    $("input.search-tokens").on('input', function () {
      let search = (<string>$(this).val()).trim().toUpperCase();
      $(".js-markets>tr").each(function () {
        let toShow = $(this).attr("id").split("_")[1].toUpperCase().includes(search);
        $(this).toggle(toShow);
      });
    });

    $(".js-markets").on("click", "tr", function (event) {
      if ($(event.target).closest("svg.fa-star").length > 0 || $(event.target).closest(".token-favorite").length > 0)
        return;
      let tokenSymbol = $(this).attr("id").split("_")[1];
      location.href = "?tokenSymbol=" + tokenSymbol;
    });

    await this.updateMarkets();
  }

  async updateMarkets() {
    $.ajax(`/api/trades/markets`).done(data => {
      let tableData = "";
      data.forEach(token => {
        let change = (100 * (token.change / token.volume));
        tableData += `<tr class="clickable-row" data-href="" id="token_${token.symbol}"><td>` + 
          `<div class="star-inner"><input class="token-favorite" id="token-favorite_${token.symbol}" type="checkbox" /><label for="token-favorite_${token.symbol}"><i class="fas fa-star"></i></label>` +
          `<span class="token-name">${token.symbol}</span></div></td>` +
          `<td><span>${token.price == 0 ? "-" : token.price.toFixed(2)}</span></td>` +
          `<td><span>${parseFloat(token.volume).toFixed(2)}</span></td>` +
          `<td><span class="${token.change > 0 ? "green-color" : token.change < 0 ? "red-color" : ""}">${token.change > 0 ? "+" : ""}${token.change == 0 ? "-" : change.toFixed(2)}</span></td>`;
      });
      $(".js-markets").html(tableData);

      this.sortMarkets();
    });
  }

  sortMarkets() {
    let v = (<any>window).Cookies.get('token-favorites');
    // restore favorite tokens from cookies
    let favoriteTokens: Array<String> = (v === undefined || v === null) ?  [] : JSON.parse(v);
    if (favoriteTokens)
      for (var i = 0; i < favoriteTokens.length; i++)
        $(`#token_${favoriteTokens[i]} input.token-favorite`).prop("checked", true);

    // sort table
    var $tbody = $(".js-markets");
    (<any>$tbody.find('tr')).sort(function(a, b) {
        if (!$('input.token-favorite', a).prop("checked") && $('input.token-favorite', b).prop("checked"))
          return 1;
        if ($('input.token-favorite', a).prop("checked") && !$('input.token-favorite', b).prop("checked"))
          return -1;
        return $(a).attr("id").split("_")[1].localeCompare($(b).attr("id").split("_")[1]);
    }).appendTo($tbody);
  }
}

export function initialize() {
  let obj = new Markets();
}


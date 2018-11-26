$(".order-amount, .order-price, .order-expires").on('input', function () {
  var $container = $(this).closest(".form-box");
  var price = Number($container.find(".order-price").val());
  var amount = Number($container.find(".order-amount").val());
  var expires = Number($container.find(".order-expires").val());
  var total = null;
  if (!price || price <= 0 || !amount || amount <= 0) {
    $container.find(".order-total").val("");
  } else {
    total = price * amount;
    $container.find(".order-total").val(total);
  }
  var isIncorrect = !total || !expires;
  $container.find(".btn-submit").prop("disabled", isIncorrect);
});

$('.js-closemodal').on('click', function (event) {
   $(event.target).closest('.modal').hide();
});



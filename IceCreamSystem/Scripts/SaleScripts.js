
//Call calculate
$("#tableProducts").change(function () {
    calculate();
    calculateUnits();
});

//Calculates total product price
function calculate() {
    var priceProducts = document.getElementsByClassName('price')
    var amountProducts = document.getElementsByClassName('amount')
    var total = document.getElementById('total')
    var priceTotal = 0.0;

    if (amountProducts.length > 0) {
        for (var i = 0; i < amountProducts.length; i++) {
            priceTotal = priceTotal + (parseFloat(priceProducts[i].innerText.replace('R$ ', '')) * parseFloat(amountProducts[i].value))
        }
        total.innerHTML = "R$ " + priceTotal;
    }
};

//Calculates total by product
function calculateUnits() {
    var priceProducts = document.getElementsByClassName('price')
    var amountProducts = document.getElementsByClassName('amount')
    var totalPricesProducts = document.getElementsByClassName('totalPrice')

    if (amountProducts.length > 0) {
        for (var i = 0; i < amountProducts.length; i++) {
            var priceTotal = 0.0;
            priceTotal = parseFloat(priceProducts[i].innerText.replace('R$ ', '')) * parseFloat(amountProducts[i].value);
            totalPricesProducts[i].innerHTML = "R$ " + priceTotal
        }
    }
};

//Put product in cookie and open Payment
$("#finish").click(function () {
    var idProducts = document.getElementsByClassName('idProd')
    var nameProducts = document.getElementsByClassName('nameProduct')
    var priceProducts = document.getElementsByClassName('price')
    var amountProducts = document.getElementsByClassName('amount')

    document.cookie = "products=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";

    document.cookie = "products=";

    if (nameProducts.length > 0) {
        for (var i = 0; i < nameProducts.length; i++) {
            document.cookie += idProducts[i].innerText + "," + nameProducts[i].innerText + "," + priceProducts[i].innerText + "," + amountProducts[i].value + "/"
        }
        document.cookie += ";path=/";
    };

    $('#debitCard').hide();
    $('#creditCard').hide();
    $('#codePayment').hide();
    $('#installment').hide();
    $("#payment").show();
});

//Put product in cookie but don't open payment
$("#exit").click(function () {
    var idProducts = document.getElementsByClassName('idProd')
    var nameProducts = document.getElementsByClassName('nameProduct')
    var priceProducts = document.getElementsByClassName('price')
    var amountProducts = document.getElementsByClassName('amount')

    document.cookie = "products=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";

    document.cookie = "products=";

    if (nameProducts.length > 0) {
        for (var i = 0; i < nameProducts.length; i++) {
            document.cookie += idProducts[i].innerText + "," + nameProducts[i].innerText + "," + priceProducts[i].innerText + "," + amountProducts[i].value + "/"
        }
        document.cookie += ";path=/";
    };
});

//Hide or show typePament
$('#payment').change(function () {

    var typePayment = $('#TypePayment').val();
    console.log(typePayment);

    if (typePayment === 'Credit') {
        $('#creditCard').show();
        $('#codePayment').show();
        $('#installment').show();
        $('#debitCard').hide();

    }
    else if (typePayment === 'Debit') {
        $('#debitCard').show();
        $('#codePayment').show();
        $('#creditCard').hide();
        $('#installment').hide();
    }
    else {
        $('#debitCard').hide();
        $('#creditCard').hide();
        $('#codePayment').hide();
        $('#installment').hide();
    }
});

//Remove items
//$(document).ready(function () {
$(document).on("click", "button.remove", function () {
    $(this).parent().parent().remove();
});


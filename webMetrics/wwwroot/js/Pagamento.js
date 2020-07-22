function SubmitBrand() {
    PagSeguroDirectPayment.getBrand({
        cardBin: $("input#txtcardNumber").val(),
        success: function (response) {
            //bandeira encontrada
            $("input#txtbrand").val(response.brand.name);
            SubmitCard();
        },
        error: function (response) {
            alert("Erro: getBrand, " + response);
        },
        complete: function (response) {
            //tratamento comum para todas chamadas
        }
    });
}
function SubmitCard() {
    var param = {
        cardNumber: $("input#txtcardNumber").val(),
        cvv: $("input#txtcvv").val(),
        expirationMonth: $("input#txtexpirationMonth").val(),
        expirationYear: $("input#txtexpirationYear").val(),
        success: function (response) {
            //token gerado, esse deve ser usado na chamada da API do Checkout Transparente
            $('input#hddtokenCard').val(response.card.token);
            GetHash();
            //$("input#btn").css("", "");
        },
        error: function (response) {
            //tratamento do erro
            alert("Erro: create card token, " + response);
        },
        complete: function (response) {
            //tratamento comum para todas chamadas
        }
    }
    //parâmetro opcional para qualquer chamada
    if ($("input#txtbrand").val() != '') {
        param.brand = $("input#txtbrand").val();
    }
    PagSeguroDirectPayment.createCardToken(param);
};

function GetHash() {
    PagSeguroDirectPayment.onSenderHashReady(function (response) {
        if (response.status == 'error') {
            console.log(response.message);
            alert("Erro: onSenderHashReady, " + response.message);
            return false;
        } else {
            var hash = response.senderHash; //Hash estará disponível nesta variável.
            $("input#hddhash").val(hash);
            GetInstallment();
        }
    });
}

var _urlSandBox = '';
function GetInstallment() {
    _amount = ($('input#Tipo').val() == "1" ? "119.00" : ($('input#Tipo').val() == "2" ? "3119.00" : "6119.00"));
    _brand = $("input#txtbrand").val();
    
    _urlCreate = _urlSandBox + '/relatorios/CreatePay?token=' + $('input#hddtokenCard').val() + "&hash=" + $('input#hddhash').val() +
        "&Cpf=" + $('input#Cpf').val() +
        "&Nome=" + $('input#Nome').val() +
        "&Sobrenome=" + $('input#Sobrenome').val() +
        "&Telefone=" + $('input#Telefone').val() +
        "&Street=" + $('input#Street').val() +
        "&Number=" + $('input#Number').val() +
        "&Complement=" + $('input#Complement').val() +
        "&District=" + $('input#District').val() +
        "&City=" + $('input#City').val() +
        "&State=" + $('input#State').val() +
        "&Country=" + $('input#Country').val() +
        "&PostalCode=" + $('input#PostalCode').val() +
        "&Tipo=" + $('input#Tipo').val() +
        "&InstallmentValue=" + _amount +
        "&DataNascimento=" + $('input#DataNascimento').val() +
        "&Email=" + $('input#Email').val();

    $.ajax
        ({
            url: _urlCreate,
            type: 'GET',
            success: function (dados) {
                if (dados == "") {
                    alert('Deu certo, aguarde a confirmação do pagamento e comemore!');
                    window.location.href = _urlSandBox + "/relatorios/Consultar";
                }
                else {
                    var resultado = dados; // Caso vá retornar alguma coisa
                    alert(dados);
                }
            },
            error: function (erro) {
                alert("Erro: getinstallments," + erro);
            }
        });    
}
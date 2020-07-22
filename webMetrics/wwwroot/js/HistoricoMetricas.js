var _url = 'https://www.influencersmetrics.com';
//var _url = 'https://localhost:44318';
//var _url = '/';

var _apiSetToken = '/facedata/setToken';
var _apiSaveData = "/facedata/setJsonData";
var _apiRemoverConsulta = "/relatorios/removerconsulta";
var _apiSobreposicao = '/facedata/getsobreposicao'
var _key = "tipo-01";
var _userId = "";

$(document).ready(function () {
    $('.date').mask('00/00/0000');
    $('.cep').mask('00000-000');

    var SPMaskBehavior = function (val) {
        return val.replace(/\D/g, '').length === 11 ? '(00) 00000-0000' : '(00) 0000-00009';
    },
        spOptions = {
            onKeyPress: function (val, e, field, options) {
                field.mask(SPMaskBehavior.apply({}, arguments), options);
            }
        };

    $('.phone').mask(SPMaskBehavior, spOptions);
});

function checkLoginState() {
    FB.getLoginStatus(function (response) {
        statusChangeCallback(response);
    });
}

function statusChangeCallback(response) {
    if (response.status == "connected") {
        document.getElementById('btnFacebook').style.display = 'none';

        //Carregar paginas
        listPages();
    }
    else {
        document.getElementById('btnFacebook').style.display = 'block';
    }
}

function listPages() {
    FB.api('/me/accounts?fields=instagram_business_account,name,access_token', function (dataface) {
        var tokens = $.grep(dataface.data, function (p) { return p.category != ''; });

        var tokensName = tokens.map(function (p) {
            return { 'instagram_business_account': p.instagram_business_account, 'name': p.name, 'access_token': p.access_token }
        });

        var str = "<select style='form-control' id='ddlPage' onchange='selectPage()'>";
        str += "<option value=''>Escolha a sua página</option>";
        $(tokensName).each(function () {
            if (this.instagram_business_account != "" && this.instagram_business_account != null) {
                str += '<option value=' + this.instagram_business_account.id + ';' + this.access_token + '>' + this.name + '</option>';
            }
        });
        str += "</select>";

        $('#cbxPaginas').html(str);
        str = "";
    });
}

instagram_business_account = "";
access_token = "";
nomePage = "";
function selectPage() {
    var e = document.getElementById("ddlPage");
    var idPage = e.options[e.selectedIndex].value;
    instagram_business_account = idPage.split(';')[0];
    access_token = idPage.split(';')[1];
    nomePage = e.options[e.selectedIndex].text;
}

function setToken() {
    if (instagram_business_account == "" || access_token == "" || nomePage == "") {
        alert('Antes de prosseguir, precisa selecionar uma página vinculada a sua conta instagram!');
        return;
    }
    $("#btnPermitir").hide();
    saveToken(instagram_business_account, access_token, nomePage);
}

function saveToken(instagram_business_account, access_token, nomePage) {
    $.ajax({
        url: _url + _apiSetToken,
        type: 'post',
        data: {
            instagram_business_account: instagram_business_account,
            access_token: access_token,
            nomePage: nomePage//,
            //userId: _userId
        },
        beforeSend: function () {
            $("#resultado").html("ENVIANDO...");
        }
    })
        .done(function (msg) {
            if (msg.indexOf("Error#") == -1) {
                msg = setInterval(function () {
                    alert('Pronto, você pode consultar os influenciadores.');
                    $('#facebookPagesModal').modal('hide');
                    clearInterval(msg);
                    window.location = 'HistoricoMetricas';
                }, 1000);
            } else {
                $("#btnPermitir").show();
                alert('Algum problema ocorreu: ' + msg.replace('Error#', ''));
            }
        })
        .fail(function (jqXHR, textStatus, msg) {
            $("#btnPermitir").show();
            alert("Fail: " + msg);
        });
}

function loadExcluir(id, origem)
{
    $("#excluir").val(id);
    $("#origem").val(origem);
    $('#ExcluirModal').modal('show');
}

function excluir(id, origem) {
    $.ajax({
        url: _url + _apiRemoverConsulta,
            type: 'post',
            data: {
                excluir: $("#excluir").val(),
                origem: $("#origem").val()
            }
        })
        .done(function (msg) {
            if (msg.indexOf("Error#") == -1) {
                msg = setInterval(function () {
                    $('#divRow' + $("#excluir").val()).css("display", "none");
                    $('#divRow' + $("#excluir").val()).removeClass("d-flex");
                    $('#ExcluirModal').modal('hide');
                    clearInterval(msg);                    
                }, 1000);
            } else {
                alert('Algum problema ocorreu: ' + msg.replace('Error#', ''));
            }
        })
        .fail(function (jqXHR, textStatus, msg) {
            alert("Fail: " + msg);
        });
}

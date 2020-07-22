var _url = 'https://www.influencersmetrics.com/facedata/';
//var _url = 'https://localhost:44318/facedata/';

var _apiSetToken = 'setTokenUsuario';
var _apiSaveData = "setJsonData";
var _key = "tipo-02";
var _userId = "";
var _clienteId = "";
var _apiStoryInsight = "SetStoryInsight";

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
        document.getElementById('boxVincularPage').style.display = 'block';

        //Carregar paginas
        listPages();
    }
    else {
        document.getElementById('btnFacebook').style.display = 'block';
        document.getElementById('boxVincularPage').style.display = 'none';
    }
}

function listPages() {
    FB.api('/me/accounts?fields=instagram_business_account,name,access_token', function (dataface) {
        var tokens = $.grep(dataface.data, function (p) { return p.category != ''; });

        var tokensName = tokens.map(function (p) {
            return { 'instagram_business_account': p.instagram_business_account, 'name': p.name, 'access_token': p.access_token }
        });

        if (tokensName.length == 0) {
            //alert('Você precisa vincular ou tornar sua conta pessoal do instagram, uma conta de negócios, a qual chamamos de Conta Business. Para saber mais, entre em seu perfil do facebook e vincule uma conta do instagram a sua página, após isto, vá em configurações de conta e altere a sua conta, para conta business.');
            alert('Você ainda não possui uma página de negócios em sua conta do facebook.Crie uma página de negócios, vincule seu instagram a esta página e retorne para refazer sua análise.');
            return;
        }

        var pagsCount = 0;
        var str = "";
        str = "<option value=''>Escolha a sua página</option>";
        $(tokensName).each(function () {
            if (this.instagram_business_account != "" && this.instagram_business_account != null) {
                pagsCount += 1;
                str += '<option value=' + this.instagram_business_account.id + ';' + this.access_token + '>' + this.name + '</option>';
            }
        });
        
        $('#ddlPage').html(str);
        str = "";

        if (pagsCount == 0) {
            alert('Você ainda não possui uma página de negócios em sua conta do facebook.Crie uma página de negócios, vincule seu instagram a esta página e retorne para refazer sua análise.');
            return;
        }
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
                nomePage: nomePage,
                userId: "",
                agenciaUserId: _userId,
                clienteId: _clienteId
            },
            beforeSend: function () {
                $("#resultado").html("ENVIANDO...");
            }
        })
        .done(function (msg) {
            if (msg.indexOf('Error#') == -1) {
                FindDatas(msg);
            } else {
                $("#btnPermitir").show();
                alert('Algum problema ocorreu: ' + msg.replace('Error#', ''));
            }
        })
        .fail(function (jqXHR, textStatus, msg) {
            $("#btnPermitir").show();
            alert(_apiSetToken + '::' + msg);
        });
}

function FindDatas(_newId) {
    FindData('?fields=biography,id,ig_id,followers_count,follows_count,media_count,name,profile_picture_url,username,website', 'Usuario', _newId);
    FindData('media?fields=caption,children,comments{username,text,id},comments_count,id,ig_id,is_comment_enabled,like_count,media_type,media_url,owner,permalink,shortcode,thumbnail_url,timestamp,username', 'Media', _newId);
    FindData('tags?fields=caption,owner,username,media_url,comments_count,like_count&limit=25', 'tags', _newId)
    FindData('insights?metric=audience_gender_age&period=lifetime', 'InsightsGenderAge', _newId);
    FindData('insights?metric=audience_city&period=lifetime', 'InsightsCity', _newId);
    FindData('insights?metric=profile_views&period=day', 'UserInsights', _newId);
    FindData('insights?metric=impressions,reach&period=week', 'UserInsights', _newId);

    /*Stories*/
    FindData('stories?fields=media_url,permalink,username,owner,media_type,shortcode', 'Stories', _newId);
}

qtdData = 0;
function FindData(_url, _nameData, _newId) {
    $('#loading').show();
    qtdData = qtdData + 1;
    FB.api('/' + instagram_business_account + '/' + _url, function (dataface) {
        saveData(JSON.stringify(dataface), _nameData, _newId);
        if (_nameData == "Stories") {
            SaveStories(dataface);
        }
        console.log(dataface);
    });
}

function SaveStories(datas) {
    jQuery.each(datas.data,
        function (id, item) {
            console.log(item);
            FB.api('/' + item.id + '/insights?metric=exits,impressions,reach,replies,taps_forward,taps_back',
                function (dataface) {
                    saveData(JSON.stringify(dataface), "dataStories");
                });
        });
}

function saveData(jsonString, _nameData, _newId) {
    $.ajax({
        url: _url + _apiSaveData,
        type: 'post',
        data: {
            json: jsonString,
            namePage: nomePage,
            key: _newId,
            nameData: _nameData
        },
        beforeSend: function () {
            //$("#resultado").html("ENVIANDO...");
        }
        })
        .done(function (msg) {
            $("#ok" + _nameData).attr("style", "display:block");
            qtdData = qtdData - 1;
            if (msg.indexOf('Error#') == -1) {
                verifyCountData();
            } else {
                alert('Aconteceu alguma coisa, ' + msg);
            }
        })
        .fail(function (jqXHR, textStatus, msg) {
            qtdData = qtdData - 1;
            alert(_apiSaveData + "::" + msg);
        });
}

function setStoryInsight(usuarioInstagram, _newId) {
    $.ajax({
        url: _url + _apiStoryInsight,
        type: 'post',
        data: {
            idUsuarioInstagram: usuarioInstagram,
            _userId: _newId
        },
        beforeSend: function () {
            
        }
    })
        .done(function (msg) {
            
        })
        .fail(function (jqXHR, textStatus, msg) {
            
        });
}

function verifyCountData() {
    if (qtdData == 0) {
        $('#loading').hide();
        msg = setInterval(function () {
            alert('Pronto, você permitiu que a agência ou marca solicitante tenha acesso aos seus resultados de influência. Para ter acesso a sua análise, entre em contato com sua agência ou clique aqui.');
            $('#facebookPagesModal').modal('hide');
            clearInterval(msg);
            $.ajax({
                url: _url + "Processado",
                type: 'post',
                data: {
                    usuario: _userId,
                    key: _key
                },
                beforeSend: function () {
                    //$("#resultado").html("ENVIANDO...");
                }
            })
                .done(function (msg) {
                    $('#facebookPagesModal').modal('hide');
                })
                .fail(function (jqXHR, textStatus, msg) {
                    //alert('Processado::'+msg);
                    $('#facebookPagesModal').modal('hide');
                });
            window.location = '/';
        }, 1000);
    }
}
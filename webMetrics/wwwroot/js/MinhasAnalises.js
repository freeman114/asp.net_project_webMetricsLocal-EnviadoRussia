var _url = 'https://www.influencersmetrics.com/facedata/';
//var _url = 'https://localhost:44318/facedata/';

var _apiSetToken = 'setToken';
var _apiSaveData = "setJsonData";
var _key = "tipo-01";
var _userId = "";

$(document).ready(function () {
    var dataX =$('.date').length;
    var cepX =$('.cep').length;
    if (dataX > 0 && cepX > 0) {
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
    }
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
            nomePage: nomePage,
            key: _userId
        },
        beforeSend: function () {
            $("#resultado").html("ENVIANDO...");
        }
    })
        .done(function (msg) {
            if (msg == '') {
                $('#loading').show();
                novaConsulta();
                //FindDatas();
            } else {
                alert('Algum problema ocorreu: ' + msg.replace('Error#', ''));
            }
        })
        .fail(function (jqXHR, textStatus, msg) {
            alert(msg);
        });
}

function FindDatas() {

    FindData('?fields=biography,id,ig_id,followers_count,follows_count,media_count,name,profile_picture_url,username,website', 'Usuario');
    FindData('media?fields=caption,children,comments{username,text,id},comments_count,id,ig_id,is_comment_enabled,like_count,media_type,media_url,owner,permalink,shortcode,thumbnail_url,timestamp,username', 'Media');
    FindData('tags?fields=caption,owner,username,media_url,comments_count,like_count&limit=25', 'tags')
    FindData('insights?metric=audience_gender_age&period=lifetime', 'InsightsGenderAge');
    FindData('insights?metric=audience_city&period=lifetime', 'InsightsCity');
    FindData('insights?metric=profile_views&period=day', 'UserInsights');
    FindData('insights?metric=impressions,reach&period=week', 'UserInsights');

    /*Stories*/
    FindData('stories?fields=media_url,permalink,username,owner,media_type,shortcode', 'Stories');
}

qtdData = 0;
function FindData(_url, _nameData) {
    qtdData = qtdData + 1;
    FB.api('/' + instagram_business_account + '/' + _url, function (dataface) {
        saveData(JSON.stringify(dataface), _nameData);
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

function saveData(jsonString, _nameData) {
    $.ajax({
        url: _url + _apiSaveData,
        type: 'post',
        data: {
            json: jsonString,
            namePage: nomePage,
            key: _userId,
            nameData: _nameData
        },
        beforeSend: function () {
            //$("#resultado").html("ENVIANDO...");
        }
    })
        .done(function (msg) {
            //$('#ok' + _nameData).show();
            $("#ok" + _nameData).attr("style", "display:block");
            qtdData = qtdData - 1;
            //$("#resultado").html(msg);
            if (msg.indexOf('Error#') == -1) {
                verifyCountData();
            } else {
                alert('Aconteceu alguma coisa, ' + msg);
            }
        })
        .fail(function (jqXHR, textStatus, msg) {
            qtdData = qtdData - 1;
            alert(msg);
        });
}


function novaConsulta() {

    $.ajax({
        url: _url + 'novaconsulta',
        type: 'post',
        data: {
            id: _userId,
            origem: 'm'
        },
        beforeSend: function () {
        }
    })
        .done(function (msg) {
            qtdData = 0;
            if (msg.indexOf('Error#') == -1) {
                verifyCountData();
            } else {
                alert('Argh, aconteceu alguma coisa, ' + msg);
            }
        })
        .fail(function (jqXHR, textStatus, msg) {
            qtdData = qtdData - 1;
            alert(msg);
        });
}

function verifyCountData() {
    if (qtdData == 0) {
        $('#loading').hide();
        msg = setInterval(function () {
            alert('Pronto, você já permitiu análise de seus dados, aguardar disponibilidade dos relatórios.');

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
                    alert(msg);
                    $('#facebookPagesModal').modal('hide');
                });

            window.location = 'minhasanalises';
        }, 1000);
    }
}

function abrirplanos() {
    $('#planosModal').modal('show');
}
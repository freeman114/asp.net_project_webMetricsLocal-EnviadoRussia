// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var _url = "https://www.influencersmetrics.com/facedata/";
var _apiSaveData = "loadJsonData";
var accessToken = "";
var instagram_business_account = "";
var _usuarioInstagram = "";

window.fbAsyncInit = function () {
    FB.init({
        appId: '220440968764019',
        cookie: false,
        xfbml: true,
        version: 'v3.1'
    });

    FB.AppEvents.logPageView();
};

(function (d, s, id) {
    var js, fjs = d.getElementsByTagName(s)[0];
    if (d.getElementById(id)) { return; }
    js = d.createElement(s); js.id = id;
    js.src = "https://connect.facebook.net/en_US/sdk.js";
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk'));

function checkLoginState() {
    FB.getLoginStatus(function (response) {
        statusChangeCallback(response);
    });
}

function statusChangeCallback(response) {
    if (response.status == 'connected') {
        accessToken = response.authResponse.accessToken;
    }
}

function listPages() {
    FB.api('/me/accounts?fields=instagram_business_account,name', function (dataface) {
        var tokens = $.grep(dataface.data, function (p) { return p.category != ''; });

        var tokensName = tokens.map(function (p) {
            return { 'instagram_business_account': p.instagram_business_account, 'name': p.name }
        });

        var str = "<select style='color:black' id='ddlPage' onchange='selectPage()'>";
        str += "<option value=''>Escolha a sua conta Instagram</option>";
        $(tokensName).each(function () {
            if (this.instagram_business_account != "" && this.instagram_business_account != null) {
                str += '<option value=' + this.instagram_business_account.id + '>' + this.name + '</option>';
            }
        });
        str += "</select>";

        $('#cbxPaginas').html(str);
        $('#divPaginas').css({ display: "block" });

        str = "";
    });
}

function selectPage() {
    //_key = document.getElementById("autorizacaoMetrica_UsuarioInstagram").value;
    //if (_key == "") {
    //    alert('Definir um perfil para analise.');
    //    return;
    //}
    var e = document.getElementById("ddlPage");
    var idPage = e.options[e.selectedIndex].value;
    instagram_business_account = idPage;
    _usuarioInstagram = e.options[e.selectedIndex].text;
}

var qtdData = 0;
function GetData(tipo, value) {
    if (tipo==1 && instagram_business_account == '') {
        alert('Selecione um perfil!');
        return;
    }
    if (tipo == 2 && instagram_business_account == '') {
        var e = document.getElementById("ddlPage");
        var idPage = e.options[0].value;
        instagram_business_account = idPage;
        if (instagram_business_account == '') {
            alert('Selecione um perfil!');
            return;
        }
    }

    if (tipo == 1) {//Insigths
        FindData('?fields=biography,id,ig_id,followers_count,follows_count,media_count,name,profile_picture_url,username,website', 'Head');
        FindData('media?fields=caption,children,comments{username,text,id},comments_count,id,ig_id,is_comment_enabled,like_count,media_type,media_url,owner,permalink,shortcode,thumbnail_url,timestamp,username', 'media');
        FindData('insights?metric=audience_gender_age&period=lifetime', 'insigthsGender');
        FindData('tags?fields=caption,owner,username,media_url,comments_count,like_count&limit=25', 'tags')
        FindData('insights?metric=audience_city&period=lifetime', 'insigthsCity');
    }

    if (tipo == 2) {//Medias
        FindData('?fields=business_discovery.username(' + value + '){followers_count,media_count,media{comments_count,like_count,caption,media_url,timestamp}}', 'Medias');
    }
}

//Precisa existir o Modal
function FindData(_url, _nameData) {
    //$('#loading-modal').modal('show');
    qtdData = qtdData + 1;
    FB.api('/' + instagram_business_account + '/' + _url, function (dataface) {
        //Enviar para o MongoDB
        document.getElementById('txtresultado').value = document.getElementById('txtresultado').value + ' ----------- ' +
            JSON.stringify(dataface);
        saveData(JSON.stringify(dataface), _nameData);
        console.log(dataface);
    });
}

function saveData(jsonString, _nameData) {
    $.ajax({
        url: _url + _apiSaveData,
        type: 'post',
        data: {
            json: jsonString,
            usuario: _usuarioInstagram,
            key: _key,
            nameData: _nameData
        },
        beforeSend: function () {
            $("#resultado").html("ENVIANDO...");
        }
    })
    .done(function (msg) {
        qtdData = qtdData - 1;
        $("#resultado").html(msg);
        if (msg.indexOf('usuario:') > -1)
        {
            //usuarioInstagram = msg.replace("usuario:", "").replace("'", "");
            verifyCountData();
        } else {
            alert('aconteceu alguma coisa, ' + msg);
        }
    })
    .fail(function (jqXHR, textStatus, msg) {
        qtdData = qtdData - 1;
        alert(msg);
    });
}

function verifyCountData() {
    if (qtdData == 0 && _usuarioInstagram != "") {
        $.ajax({
            url: "https://www.influencersmetrics.com/facedata/Processado",
            type: 'post',
            data: {
                usuario: '',
                key: _key
            },
            beforeSend: function () {
                $("#resultado").html("ENVIANDO...");
            }
        })
            .done(function (msg) {
                $("#resultado").html(msg);
                $('#loading-modal').modal('hide');                
            })
            .fail(function (jqXHR, textStatus, msg) {
                alert(msg);
                $('#loading-modal').modal('hide');
            });
    }
}
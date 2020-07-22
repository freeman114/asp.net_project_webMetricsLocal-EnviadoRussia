//USANDO GEOLOCALIZAÇÃO
//http://www.linhadecodigo.com.br/artigo/3653/usando-geolocalizacao-com-html5.aspx

var map;
var infowindow;
var lat;
var lng;
var markers = [];

function getLocation() {
    //if (navigator.geolocation) {
        setPosition();
        //navigator.geolocation.getCurrentPosition(setPosition, showError);
    //}
    //else { showAlert("Geolocalização não é suportada nesse browser."); }
}

function showAlert(message) {
    alert(message);
}

function showError(error) {
    console.log(JSON.stringify(error));
    switch (error.code) {
        case error.PERMISSION_DENIED:
            showAlert("Usuário rejeitou a solicitação de Geolocalização.");
            break;
        case error.POSITION_UNAVAILABLE:
            showAlert("Localização indisponível.");
            break;
        case error.TIMEOUT:
            showAlert("O tempo da requisição expirou.");
            break;
        case error.UNKNOWN_ERROR:
            showAlert("Algum erro desconhecido aconteceu.");
            break;
    }
}

function setPosition(position) {
    //lat = position.coords.latitude;
    //lng = position.coords.longitude;
    
    InitMap();
}

function InitMarkers(mks)
{
    markers = mks;
}

function InitMap() {
    if (markers.length > 0)
    {
        lat = parseFloat(markers[0].lat.replace(",","."));
        lng = parseFloat(markers[0].lng.replace(",", "."));
    } else {
        return;
    }

    var pyrmont = { lat: lat, lng: lng };

    map = new google.maps.Map(document.getElementById('map'), {
        center: pyrmont,
        zoom: 6
    });

    var latlon = new google.maps.LatLng(lat, lng)
    var me = new google.maps.Marker(
        {
            position: latlon,
            map: map,
            title: "Você está Aqui!",
     //       icon: 'images/markers/marker-me.png',
            animation: google.maps.Animation.DROP,
        });

    infowindow = new google.maps.InfoWindow();
    /*
    var service = new google.maps.places.PlacesService(map);
    service.textSearch({
        location: pyrmont,
        // rankBy: google.maps.places.RankBy.DISTANCE,
        query: 'bancas de jornal'
    }, callback);
    */

    //var markers = [{ 'title': '1', 'lat': '-23,532800071062', 'lng': '-46,645211874217', 'description': 'Avenida Rio Branco' }, { 'title': 'São Paulo', 'lat': '-23,5312438', 'lng': '-46,6447949', 'description': 'Espaço Cultural Porto Seguro' }, { 'title': 'São Paulo (São Paulo, Brazil)', 'lat': '-23,621689', 'lng': '-46,698037', 'description': 'Teatro MorumbiShopping' }, { 'title': 'São Paulo', 'lat': '-23,558839069445', 'lng': '-46,662360327921', 'description': 'Teatro Renaissance' }, { 'title': '', 'lat': '-23,531627227133', 'lng': '-46,64472403983', 'description': 'Porto Seguro' }, { 'title': '', 'lat': '-22,975022845795', 'lng': '-43,228199869432', 'description': 'Teatro Vannucci' }, { 'title': 'SÃO PAULO', 'lat': '-23,560851834481', 'lng': '-46,694556532803', 'description': 'Teatro Cetip' }, { 'title': '', 'lat': '-22,9103879', 'lng': '-43,1771146', 'description': 'Fenaseg' }, { 'title': 'Bertioga', 'lat': '-23,789325540845', 'lng': '-46,012912473359', 'description': 'Riviera de São Lourenço' }, { 'title': 'São Paulo, Brazil', 'lat': '-23,515537696722', 'lng': '-46,617853894615', 'description': 'Shopping Center Norte' }, ];;
    for (i = 0; i < markers.length; i++) {
        var data = markers[i]
        var myLatlng = new google.maps.LatLng(data.lat.replace(",","."), data.lng.replace(",","."));
        //alert(myLatlng);
        var marker = new google.maps.Marker({
            position: myLatlng,
            map: map,
            title: data.title,
            //icon: data.image,            
            animation: google.maps.Animation.DROP
        });
        (function (marker, data) {
            google.maps.event.addListener(marker, "click", function (e) {
                //alert('addListener');
                infoWindow.setContent(data.description);
                infoWindow.open(map, marker);
            });
        })(marker, data);
    }

    google.maps.event.addListener(me, 'click', function () {
        infowindow.setContent('Você está Aqui!');
        infowindow.open(map, this);
    });
}


function callback(results, status) {
    //alert(status)
    console.log(results.length);
    if (status === google.maps.places.PlacesServiceStatus.OK) {
        for (var i = 0; i < results.length; i++) {
            createBancaMarker(results[i]);
        }
    }
}

function createBancaMarker(place) {
    var placeLoc = place.geometry.location;
    var marker = new google.maps.Marker({
        map: map,
        position: place.geometry.location,
        animation: google.maps.Animation.DROP
    });

    google.maps.event.addListener(marker, 'click', function () {
        infowindow.setContent(place.name);
        infowindow.open(map, this);
    });
}
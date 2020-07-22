var posicaoInicial = $('#header-1').position().top;
$(document).scroll(function () {
    var posicaoScroll = $(document).scrollTop();
    console.log(posicaoScroll);
    if(posicaoScroll>=95){
    	$('#header-1').addClass("menu-fixed");
    	$('.bx_cinza_navDireita').css({"top":"100px"});
    }else{
    	$('#header-1').removeClass("menu-fixed");
    	$('.bx_cinza_navDireita').css({"top":"20px"});
    }
})
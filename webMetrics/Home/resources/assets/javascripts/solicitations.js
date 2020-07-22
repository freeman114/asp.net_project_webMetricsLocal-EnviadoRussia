var array_inputs = [];


var nPage = 0;
var nInd  = 1;

var addInputArray = function(that)
{
  
  if(array_inputs[$(that).data('page')] === undefined)
  {
    array_inputs[$(that).data('page')] = [];
    
  }
  
  if(array_inputs[$(that).data('page')][$(that).data('ind')] === undefined)
  {
    array_inputs[$(that).data('page')][$(that).data('ind')] = [];
  }
  
  array_inputs[$(that).data('page')][$(that).data('ind')] = ($(that).val() == "" ? "NULL" : $(that).val());   
  
  
  $('#array_inputs').val(JSON.stringify(array_inputs));
}

var goToPage = function(page)
{

  $.each($('#box-inputs :not([data-page="'+page+'"])'), function(ind,val){
      
    if(!$(val).hasClass('fa-plus'))
    {
      $(val).parent().addClass('d-none');
    }
    
  });
  
  $.each($('[data-page="'+page+'"]'), function(ind,val){

    $(val).parent().removeClass('d-none');
    
  });

  $('#current_page').val(page);

  $('#pagination .page-item').removeClass('active');
  $('#pagination .page-item').eq(page).addClass('active');

}

var addInput = function(that)
{

  goToPage(nPage);
  
  var total_inputs = parseInt($('#total_inputs').val());
  total_inputs++;
  $('#box-inputs').prepend('<div class="col-md-4 pr-2 pl-2 form-group pb-0 mb-3"><input data-page="'+nPage+'" data-ind="'+nInd+'" type="text" class="text-center form-control" id="profile'+total_inputs+'" name="profile[]" placeholder="@"></div>');
  $('#total_inputs').val(total_inputs);
  
  addInputArray($('#profile'+total_inputs));
  
  var total_pages = Math.ceil(total_inputs / 9);
  
  $('#total_pages').val(total_pages);
  
  nInd++;
  
  if(nInd >= 9)
  {
    nInd = 0;
  }

  if(nPage > 0 && array_inputs[nPage].length == 1)
  {
    $.each($('[data-page="'+(nPage - 1)+'"]'), function(ind,val){
      
      $(val).parent().addClass('d-none');
      
    });
  }
  
  if(array_inputs[nPage].length == 9)
  {
    
    nPage++;
    
  }
  
  $('#current_page').val(total_pages);
  
  
  $('#nPage').val(nPage);
  $('#nInd').val(nInd);
  
  window.renderPagination(array_inputs);
  
  
}

$(document).on('blur','input[name="profile[]"]',function()
{
  addInputArray(this);

})

window.renderPagination = function(items)
{ 
  var current_page = $('#current_page').val();

  console.log('RENDER PAGINATION!');
  
  var pages = '';
  
  $.each(items, function(ind,val)
  {
    pages += '<li class="page-item '+(current_page == (ind + 1) ? 'active' : '')+'"><a class="page-link border-0 "href="javascript:;" onclick="goToPage('+ind+');" style="cursor:default !important;">'+(ind + 1)+'</a></li>';
  })
  
  
  var template_pagination =
  '  <ul class="pagination pagination-sm justify-content-center">'+
  pages+
  '  </ul>';
  
  if(items.length > 1)
  {
    $('#pagination').html(template_pagination);
  }

  
}
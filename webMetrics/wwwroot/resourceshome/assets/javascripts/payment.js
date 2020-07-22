$(document).ready(function()
{
  $('.date').mask('00/00/0000');
  $('.cep').mask('00000-000');
  $('.ddd').mask('00');
  
  var SPMaskBehavior = function (val) {
    return val.replace(/\D/g, '').length === 11 ? '00000-0000' : '0000-00009';
  },
  spOptions = 
  {
    onKeyPress: function(val, e, field, options) 
    {
      field.mask(SPMaskBehavior.apply({}, arguments), options);
    }
  };
  
  $('.phone').mask(SPMaskBehavior, spOptions);
  $('.cpf').mask('000.000.000-00', {reverse: true});
  $('.card-number').mask('0000 0000 0000 0000');
  
});
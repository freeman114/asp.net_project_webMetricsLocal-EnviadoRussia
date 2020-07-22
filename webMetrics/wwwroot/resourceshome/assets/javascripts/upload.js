window.onCancel = function(e) {
  console.log("Cancel");
  var index = $(this).parents("li").data("index");
  $(this).parents("form").find(".upload").upload("abort", parseInt(index, 10));
}
window.onCancelAll = function(e) {
  console.log("Cancel All");
  $(this).parents("form").find(".upload").upload("abort");
}
window.onBeforeSend = function(formData, file) {
  console.log("Before Send");
  // return (file.name.indexOf(".jpg") < -1) ? false : formData; // cancel all jpgs
  return formData;
}
window.onQueued = function(e, files) {
  console.log("Queued");
  var html = '';
  for (var i = 0; i < files.length; i++) {
    html += '<li data-index="' + files[i].index + '"><span class="content"><span class="file">' + files[i].name + '</span><span class="cancel btn btn-danger btn-sm text-white"><i class="fas fa-times"></i></span><span class="progress">Carregado</span></span><span class="bar"></span></li>';
  }
  $(this).parents("form").find(".filelist.queue")
  .append(html);
  $('.filelists').show();
}
window.onStart = function(e, files) {
  console.log("Start");
  $(this).parents("form").find(".filelist.queue")
  .find("li")
  .find(".progress").text("Waiting");
}
window.onComplete = function(e) {
  console.log("Complete");
  if($('#btn-ok').length == 0)
  {
    $('.filelists').append('<a href="javascript:;" id="btn-ok" class="mt-3 btn btn-warning btn-sm" data-dismiss="modal">OK</a>');
  }
}
window.onFileStart = function(e, file) {
  console.log("File Start");
  $(this).parents("form").find(".filelist.queue")
  .find("li[data-index=" + file.index + "]")
  .find(".progress").text("0%");
}
window.onFileProgress = function(e, file, percent) {
  console.log("File Progress");
  var $file = $(this).parents("form").find(".filelist.queue").find("li[data-index=" + file.index + "]");
  $file.find(".progress").text(percent + "%")
  $file.find(".bar").css("width", percent + "%");
}
window.onFileComplete = function(e, file, response) {
  console.log("File Complete");
  if (response.trim() === "" || response.toLowerCase().indexOf("error") > -1) {
    $(this).parents("form").find(".filelist.queue")
    .find("li[data-index=" + file.index + "]").addClass("error")
    .find(".progress").text(response.trim());
  } else {
    var $target = $(this).parents("form").find(".filelist.queue").find("li[data-index=" + file.index + "]");
    $target.find(".file").text(file.name);
    $target.find(".progress").text('Upload feito com sucesso!');
    $target.find(".cancel").remove();
    $target.appendTo( $(this).parents("form").find(".filelist.complete") );
  }
}
window.onFileError = function(e, file, error) {
  console.log("File Error");
  console.log(e,file,error);
  $(this).parents("form").find(".filelist.queue")
  .find("li[data-index=" + file.index + "]").addClass("error")
  .find(".progress").text("Erro: Arquivo Inválido!");
}
window.onChunkStart = function(e, file) {
  console.log("Chunk Start");
}
window.onChunkProgress = function(e, file, percent) {
  console.log("Chunk Progress");
}
window.onChunkComplete = function(e, file, response) {
  console.log("Chunk Complete");
}
window.onChunkError = function(e, file, error) {
  console.log("Chunk Error");
}

$(".upload").upload({
  action: "images?token="+localStorage.getItem('token'),
  // action:'/resources/views/upload-target.php',
  chunked: true,
  // autoUpload: false,
  beforeSend: window.onBeforeSend,
  // multiple: false,
  label: 'Arraste a sua foto aqui, ou <br><a href="javascript:;" class="btn btn-blue mt-4 text-dark btn-sm">CARREGAR FOTO DO COMPUTADOR</a>',
})
.on("start.upload", window.onStart)
.on("complete.upload", window.onComplete)
.on("filestart.upload", window.onFileStart)
.on("fileprogress.upload", window.onFileProgress)
.on("filecomplete.upload", window.onFileComplete)
.on("fileerror.upload", window.onFileError)
.on("chunkstart.upload", window.onChunkStart)
.on("chunkprogress.upload", window.onChunkProgress)
.on("chunkcomplete.upload", window.onChunkComplete)
.on("chunkerror.upload", window.onChunkError)
.on("queued.upload", window.onQueued);

$(".filelist.queue").on("click", ".cancel", window.onCancel);
$(".cancel_all").on("click", window.onCancelAll);
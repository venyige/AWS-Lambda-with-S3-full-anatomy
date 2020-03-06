function readURL(input) {
  if (input.files && input.files[0]) {
    console.log(Array.from(input.files).map(function(iii){return iii.name;}).join("<br>"));
    
      $('.file-upload-wrap').hide();
      $('.file-upload-content').show();
      $('.file-upload-files').html("Selected files: " + Array.from(input.files).map(function(iii){return iii.name;}).join(", "));
      $('.file-title').html(input.files[1]?"Selected files":"Selected file");
     
  } else {
    removeUpload();
  }
}

function removeUpload() {
  $('.file-upload-input').replaceWith($('.file-upload-input').clone());
  $('.file-upload-content').hide();
  $('.file-upload-wrap').show();
}
$('.file-upload-wrap').bind('dragover', function () {
        $('.file-upload-wrap').addClass('file-dropping');
    });
    $('.file-upload-wrap').bind('dragleave', function () {
        $('.file-upload-wrap').removeClass('file-dropping');
});


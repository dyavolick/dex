var gulp = require('gulp'),
  htmlclean = require('gulp-htmlclean'),
  uglify = require('gulp-uglify'),
  browserify = require("browserify");
var source = require('vinyl-source-stream');

// folders
var folders = {
  root: "./wwwroot/"
};

gulp.task('browserify', function () {
  return browserify('./scripts/ledger.js')
    .bundle()
    // Передаем имя файла, который получим на выходе, vinyl-source-stream
    .pipe(source('bundle.js'))
    .pipe(gulp.dest('./wwwroot/js'));
});

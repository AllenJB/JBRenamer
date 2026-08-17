#!/bin/sh
filenames=(
  'Filename.txt'
  'Filename:with:colons.txt'
  'Filename,with,commas.txt'
  'Filename.with.dots.txt'
  'Filename_with_long.extension'
  'Filename_with_no_extension'
  'Filename?with?questionmark.txt'
  'Filename with spaces.txt'
  'Filename%20with%20encoding-like.txt'
  'Filename+with+plus.txt'
  'Filename with trailing space.txt '
  'Filename without extension'
)
  
dirnames=(
  'Directory with spaces'
  'Directory with trailing space '
  'Directory.with.dots'
)

for file in "${filenames[@]}"; do
  touch "$file"
done

for dir in "${dirnames[@]}"; do
  mkdir "$dir"
done

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
)

for file in "${filenames[@]}"; do
  touch "$file"
done

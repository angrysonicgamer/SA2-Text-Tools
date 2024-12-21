# SA2 Text Tools

Simple console tools for editing Sonic Adventure 2 text files via PRS-JSON (or CSV in case of cutscenes) export/import. Basically an alternative to SA Tools but for personal use, at least for now.

## Usage
Drag and drop a compressed PRS file to export the data into JSON file (or CSV in case of cutscenes).
Drag and drop an edited file with extracted data to create a new PRS file with new data.
It will be saved in the "New files" subfolder.

## CMD usage
`SA2CutsceneTextTool filename`
`SA2MessageTextTool filename`
`SA2SubtitlesTimingTool filename`

The tools support Windows-1251, Windows-1252 and Shift-JIS encodings and both little endian and big endian byte orders.
Edit AppConfig.json to set the settings you want.

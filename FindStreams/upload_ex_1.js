const fleekStorage = require('@fleekhq/fleek-storage-js')
const fs = require('fs')

filen=      'blobs/example_000001.zip';
fs.readFile(filen, async (error, fileData) => {
  console.log(error)
  const uploadedFile = await fleekStorage.upload({
    apiKey: myapikey,
    apiSecret: myapisecret,
    key: filen,
    ContentType: 'application/zip',
    data: fileData,
    httpUploadProgressCallback: (event) => {
      console.log(Math.round(event.loaded/event.total*100)+ '% done');
    }
  });
})

filen=      'blobs/example_000002.zip';
fs.readFile(filen, async (error, fileData) => {
  console.log(error)
  const uploadedFile = await fleekStorage.upload({
    apiKey: myapikey,
    apiSecret: myapisecret,
    key: filen,
    ContentType: 'application/zip',
    data: fileData,
    httpUploadProgressCallback: (event) => {
      console.log(Math.round(event.loaded/event.total*100)+ '% done');
    }
  });
})



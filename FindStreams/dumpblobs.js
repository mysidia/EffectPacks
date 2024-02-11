const fleekStorage = require('@fleekhq/fleek-storage-js')
const fs = require('fs')


async function writeFlist()
{
set1= await fleekStorage.listFiles({apiKey: myapikey, apiSecret: myapisecret, bucket: mybucketid, key: 'blobs/',  getOptions: ['key', 'hash', 'publicUrl'],  prefix: 'rhmeta/'  })


set2= await fleekStorage.listFiles({apiKey:myapikey, apiSecret: myapisecret, bucket: mybucketid, key: 'blobs/',  getOptions: ['key', 'hash', 'publicUrl'],  prefix: 'blobs/'  })

set3= await fleekStorage.listFiles({apiKey:myapikey, apiSecret: myapisecret, bucket: mybucketid, key: 'blobs/',  getOptions: ['key', 'hash', 'publicUrl'],  prefix: 'patch/'  })


combined = set1.concat(set2).concat(set3)

fs.writeFileSync("buckets.txt", JSON.stringify(combined))
}

writeFlist()


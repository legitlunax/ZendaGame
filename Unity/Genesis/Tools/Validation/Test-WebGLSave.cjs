const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const path = require('node:path');
let callback;
let calls = 0;
const library = {};
const context = {
  LibraryManager: { library },
  mergeInto: Object.assign,
  Module: {},
  FS: { syncfs(populate, cb) { assert.equal(populate, false); calls++; callback = cb; } },
  console
};
vm.runInNewContext(fs.readFileSync(path.resolve(__dirname, '../../Assets/Plugins/WebGL/GenesisSave.jslib'), 'utf8'), context);
library.GenesisSyncSave();
assert.equal(calls, 1);
library.GenesisSyncSave();
library.GenesisSyncSave();
assert.equal(calls, 1, 'overlapping requests must not start parallel syncs');
callback(null);
assert.equal(calls, 2, 'pending changes flushed after first sync');
callback(null);
assert.equal(context.Module.genesisSaveSyncing, false);
library.GenesisSyncSave();
assert.equal(calls, 3, 'later autosave can sync again');
callback(null);
console.log('PASS 6 WebGL save-flush queue checks (mock filesystem; not a browser persistence test).');
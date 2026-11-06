mergeInto(LibraryManager.library, {
  GenesisSyncSave: function () {
    // Serialize sync requests so overlapping autosaves cannot race browser storage.
    if (Module.genesisSaveSyncing) {
      Module.genesisSavePending = true;
      return;
    }
    Module.genesisSaveSyncing = true;
    var sync = function () {
      Module.genesisSavePending = false;
      FS.syncfs(false, function (error) {
        if (error) console.error('Genesis browser save failed:', error);
        if (Module.genesisSavePending) sync();
        else Module.genesisSaveSyncing = false;
      });
    };
    sync();
  }
});
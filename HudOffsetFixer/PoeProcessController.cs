using System;
using System.Diagnostics;
using System.Linq;
using HudOffsetFixer.MemorySections;
using PoeHUD.Controllers;
using PoeHUD.Framework;
using PoeHUD.Poe;

namespace HudOffsetFixer
{
    public class PoeProcessController
    {
        private Offsets _offs;
        private GameController _gameController;
        private Memory _memory;
        private MemorySectionsProcessor _memorySectionsProcessor;

        public bool ConnectToProcess()
        {
            LogWindow.LogMessage("Connecting to process...");
            var pid = FindPoeProcess(out _offs);

            if (pid == 0)
            {
                LogWindow.LogError("Path of Exile is not running.");
                return false;
            }

            try
            {
                _memory = new Memory(_offs, pid);
                _offs.DoPatternScans(_memory);
                _gameController = new GameController(_memory);
                _memorySectionsProcessor = new MemorySectionsProcessor();
                _memorySectionsProcessor.UpdateProcessInformations(_memory.procHandle);
            }
            catch (Exception e)
            {
                LogWindow.LogError($"Error while creating instance of GameController: {e}");
                return false;
            }
       
            LogWindow.LogMessage("Connected to process");
            return true;
        }

        private static int FindPoeProcess(out Offsets offs)
        {
            var clients = Process.GetProcessesByName(Offsets.Regular.ExeName).Select(p => Tuple.Create(p, Offsets.Regular)).ToList();
            if (clients.Count > 0)
            {
                offs = clients[0].Item2;
                return clients[0].Item1.Id;
            }

            offs = null;
            return 0;
        }
    }
}

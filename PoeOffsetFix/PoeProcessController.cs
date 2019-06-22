using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using PoeHUD.Controllers;
using PoeHUD.Framework;
using PoeHUD.Poe;
using PoeOffsetFix;

namespace HudBotWPF.Core
{
    public class PoeProcessController
    {
        private Offsets _offs;
        private GameController _gameController;
        private Memory _memory;

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
            clients.AddRange(Process.GetProcessesByName(Offsets.Steam.ExeName).Select(p => Tuple.Create(p, Offsets.Steam)));
            var ixChosen = clients.Count > 1 ? chooseSingleProcess(clients) : 0;

            if (clients.Count > 0 && ixChosen >= 0)
            {
                offs = clients[ixChosen].Item2;
                return clients[ixChosen].Item1.Id;
            }

            offs = null;
            return 0;
        }

        private static int chooseSingleProcess(List<Tuple<Process, Offsets>> clients)
        {
            var o1 = $"Yes - process #{clients[0].Item1.Id}, started at {clients[0].Item1.StartTime.ToLongTimeString()}";
            var o2 = $"No - process #{clients[1].Item1.Id}, started at {clients[1].Item1.StartTime.ToLongTimeString()}";
            const string o3 = "Cancel - quit this application";

            var answer = MessageBox.Show(null, string.Join(Environment.NewLine, o1, o2, o3),
                "Choose a PoE instance to attach to", MessageBoxButtons.YesNoCancel);

            return answer == DialogResult.Cancel ? -1 : answer == DialogResult.Yes ? 0 : 1;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace MasterMindLibrary
{
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)] void SomeoneWon(CallbackInfo info);
        [OperationContract(IsOneWay = true)] void SomeoneJoined(Dictionary<string, int> players);
    }

    public enum Colors { Red = 0, Green, Blue, Yellow, Pink, Purple }

    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface ICodeMaker
    {
        [OperationContract] Tuple<bool?, List<bool?>> IsCorrect(List<Colors> guess, string name);
        List<Colors> correctSequence { [OperationContract] get; [OperationContract] set; }
        [OperationContract] bool ToggleCallbacks();
        [OperationContract] string HasSomeoneWon(string name);
        Dictionary<string, int> playerNames { [OperationContract] get; [OperationContract] set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CodeMaker : ICodeMaker
    {
        private string someoneWon = "";
        public List<Colors> correctSequence { get; set; }
        private HashSet<ICallback> callbacks = null;
        const int NUM_COLORS = 4;
        public Dictionary<string, int> playerNames { get; set; }
        public CodeMaker()
        {
            callbacks = new HashSet<ICallback>();
            playerNames = new Dictionary<string, int>();
            Random rng = new Random();
            correctSequence = new List<Colors>();
            for (int i = 0; i < NUM_COLORS; i++)
            {
                correctSequence.Add((Colors)rng.Next(0, (int)Colors.Purple));
            }

            Console.WriteLine("Sequence is...");
            foreach (Colors color in correctSequence)
            {
                Console.WriteLine(color.ToString());
            }
        }

        public string HasSomeoneWon(string name)
        {
            if (playerNames.ContainsKey(name) || name == "exists")
            {
                return "exists";
            }
            else
            {
                playerNames.Add(name, 0);
                updatePlayerList();
                return someoneWon;
            }
        }

        public Tuple<bool?, List<bool?>> IsCorrect(List<Colors> guess, string name)
        {
            Tuple<bool?, List<bool?>> returnValues;
            List<bool?> hints = new List<bool?>();
            try
            {
                if (someoneWon == "")
                {
                    
                    bool correct = true;

                    for (int i = 0; i < guess.Count; i++)
                    {
                        if (guess[i] != correctSequence[i])
                        {
                            correct = false;
                            if (correctSequence.Contains(guess[i]))
                            {
                                hints.Add(false);
                            }
                            else
                            {
                                hints.Add(null);
                            }
                        }
                        else
                        {
                            hints.Add(true);
                        }
                    }


                    if (correct)
                    {
                        someoneWon = name;
                        updateAllClients(name);
                    }
                    this.playerNames[name] += 1;
                    updatePlayerList();
                    returnValues = new Tuple<bool?, List<bool?>>(correct, hints);
                    return returnValues;
                }
                else
                {
                    updateAllClients(someoneWon, true);
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public bool ToggleCallbacks()
        {
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();

            if (callbacks.Contains(cb))
            {
                callbacks.Remove(cb);
                return false;
            }
            else
            {
                callbacks.Add(cb);
                return true;
            }
        }

        private void updatePlayerList()
        {
            foreach (ICallback cb in callbacks)
                cb.SomeoneJoined(this.playerNames);
        }

        private void updateAllClients(string name, bool someoneWon = false)
        {
            CallbackInfo info = new CallbackInfo(correctSequence, name, someoneWon);

            foreach (ICallback cb in callbacks)
                cb.SomeoneWon(info);
        }
    }
}

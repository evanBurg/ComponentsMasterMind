using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace MasterMindLibrary
{
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)] void SomeoneWon(CallbackInfo info);
    }

    public enum Colors { Red = 0, Green, Blue, Yellow, Pink, Purple }

    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface ICodeMaker
    {
        [OperationContract] bool? IsCorrect(List<Colors> guess, string name);
        List<Colors> correctSequence { [OperationContract] get; [OperationContract] set; }
        [OperationContract] bool ToggleCallbacks();
        [OperationContract] string HasSomeoneWon();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CodeMaker : ICodeMaker
    {
        private string someoneWon = "";
        public List<Colors> correctSequence { get; set; }
        private HashSet<ICallback> callbacks = null;
        const int NUM_COLORS = 4;
        public CodeMaker()
        {
            callbacks = new HashSet<ICallback>();
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

        public string HasSomeoneWon()
        {
            return someoneWon;
        }

        public bool? IsCorrect(List<Colors> guess, string name)
        {
            try
            {
                if (someoneWon == "")
                {
                    string help = "";
                    bool correct = true;

                    for(int i = 0; i < guess.Count; i++)
                    {
                        if(guess[i] != correctSequence[i])
                        {
                            correct = false;
                        }
                        else
                        {
                            if (correctSequence.Contains(guess[i]))
                            {
                                help += String.Format("The sequence contains {0} but it is in the incorrect positon\n", guess[i]);
                            }
                            else
                            {
                                help += String.Format("The sequence contains {0} and it is in the correct positon\n", guess[i]);
                            }
                        }
                    }

                    

                    if (correct)
                    {
                        someoneWon = name;
                        updateAllClients(name);
                    }
                    return correct;
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

        private void updateAllClients(string name, bool someoneWon = false)
        {
            CallbackInfo info = new CallbackInfo(correctSequence, name, someoneWon);

            foreach (ICallback cb in callbacks)
                cb.SomeoneWon(info);
        }
    }
}

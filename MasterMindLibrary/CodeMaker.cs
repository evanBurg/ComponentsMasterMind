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
        [OperationContract] bool IsCorrect(List<Colors> guess, string name);
        List<Colors> correctSequence { [OperationContract] get; [OperationContract] set; }
        [OperationContract] bool ToggleCallbacks();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CodeMaker : ICodeMaker
    {
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

        public bool IsCorrect(List<Colors> guess, string name)
        {
            var firstNotSecond = correctSequence.Except(guess).ToList();
            var secondNotFirst = guess.Except(correctSequence).ToList();
            bool correct = !firstNotSecond.Any() && !secondNotFirst.Any();
            if (correct)
            {
                updateAllClients(name);
            }
            return correct;
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

        private void updateAllClients(string name)
        {
            CallbackInfo info = new CallbackInfo(correctSequence, name);

            foreach (ICallback cb in callbacks)
                cb.SomeoneWon(info);
        }
    }
}

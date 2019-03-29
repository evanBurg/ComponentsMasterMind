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
        [OperationContract(IsOneWay = true)] void UpdateGui(CallbackInfo info);
    }

    public enum Colors { Red = 0, Green, Blue, Yellow, Purple, Orange }
    
    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface ICodeMaker
    {
        [OperationContract] bool IsCorrect(List<Colors> guess);
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
            for(int i = 0; i < NUM_COLORS; i++)
            {
                correctSequence.Add((Colors)rng.Next(0, (int)Colors.Orange));
            }
        }
        
        public bool IsCorrect(List<Colors> guess)
        {
            var firstNotSecond = correctSequence.Except(guess).ToList();
            var secondNotFirst = guess.Except(correctSequence).ToList();
            return !firstNotSecond.Any() && !secondNotFirst.Any();
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

        private void updateAllClients(bool emptyHand)
        {
            CallbackInfo info = new CallbackInfo(correctSequence);

            foreach (ICallback cb in callbacks)
                cb.UpdateGui(info);
        }
    }
}

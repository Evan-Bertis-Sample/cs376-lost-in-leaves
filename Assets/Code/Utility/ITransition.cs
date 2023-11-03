using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace CurlyUtility
{
    public interface ITransition<T>
    {
        public void PrepareTransition(T value);
        public Task Transition(T value);
        public void EndTransition(T value);

        public async Task Play(T value)
        {
            PrepareTransition(value);
            await Transition(value);
            EndTransition(value);
        }
    }
}


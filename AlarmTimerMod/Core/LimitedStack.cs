using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmTimerMod.Core {

    public class LimitedStack<T> where T : class {
        private readonly LinkedList<T> stack;
        private readonly int maxSize;

        public LimitedStack(int maxSize) {
            stack = new LinkedList<T>();
            this.maxSize = maxSize;
        }

        public void Push(T item) {
            stack.AddFirst(item);

            // Remove the oldest item if the size limit is exceeded
            if (stack.Count > maxSize) {
                stack.RemoveLast();
            }
        }

        public T PopOrNull() {
            if (stack.Count == 0)
                return default;

            T value = stack.First.Value;
            stack.RemoveFirst();
            return value;
        }

        public T Pop() {
            var result = PopOrNull();
            return result == null ? throw new InvalidOperationException("Stack is empty") : result;
        }

        public T Peek() {
            if (stack.Count == 0)
                throw new InvalidOperationException("Stack is empty");

            return stack.First.Value;
        }

        public int Count => stack.Count;

        public void Clear() => stack.Clear();
    }

}

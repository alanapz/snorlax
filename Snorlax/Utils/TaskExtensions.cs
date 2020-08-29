using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Shared.App.Utils
{
    static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable OnAnyThread(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable<TResult> OnAnyThread<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable<TResult> OnSameThread<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(true);
        }
    }
}

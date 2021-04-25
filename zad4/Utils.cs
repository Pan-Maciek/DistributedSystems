using System.Threading.Tasks;

namespace zad4 {
    public static class Utils {
        public static Task<(bool Finished, T Value)[]> WhenAllFinished<T>(Task<T>[] tasks) {
            return Task<(bool, T)[]>.Factory.StartNew(() => {
                var taskCount = tasks.Length;
                var tasksCopy = new Task<T>[taskCount];
                tasks.CopyTo(tasksCopy, 0);
                var result = new (bool Finished, T Value)[taskCount];

                while (true) {
                    var done = 0;
                    for (var i = 0; i < taskCount; i++) {
                        switch (tasksCopy[i]) {
                            case null:
                                done++;
                                continue;
                            case {IsCanceled: true}:
                                tasksCopy[i] = null;
                                result[i].Finished = false;
                                result[i].Value = default;
                                done++;
                                break;
                            case {IsCompleted:true, Result: var taskResult}:
                                tasksCopy[i] = null;
                                result[i].Value = taskResult;
                                result[i].Finished = true;
                                done++;
                                break;
                        }
                    }
                    
                    if (done == taskCount) return result;
                }
            });
        }
    }
}
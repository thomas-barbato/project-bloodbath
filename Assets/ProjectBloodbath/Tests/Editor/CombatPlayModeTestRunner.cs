using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace ProjectBloodbath.Tests.Editor
{
    public static class CombatPlayModeTestRunner
    {
        private const string LogPrefix = "[Project Bloodbath Tests]";

        [InitializeOnLoadMethod]
        private static void RegisterReporter()
        {
            TestRunnerApi.RegisterTestCallback(new Reporter());
        }

        [MenuItem(
            "Tools/Project Bloodbath/Tests/Run Combat PlayMode Tests")]
        public static void Run()
        {
            TestRunnerApi api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.PlayMode,
                assemblyNames = new[]
                {
                    "ProjectBloodbath.Tests.PlayMode"
                }
            }));
        }

        [MenuItem(
            "Tools/Project Bloodbath/Tests/Run Skill Asset EditMode Tests")]
        public static void RunSkillAssetTests()
        {
            TestRunnerApi api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[]
                {
                    "ProjectBloodbath.Tests.Editor"
                }
            }));
        }

        private sealed class Reporter : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {
                Debug.Log($"{LogPrefix} Combat PlayMode tests started.");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                Debug.Log(
                    $"{LogPrefix} Finished: {result.TestStatus}; " +
                    $"passed={result.PassCount}; failed={result.FailCount}; " +
                    $"skipped={result.SkipCount}.");
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.HasChildren)
                {
                    return;
                }

                string details = string.IsNullOrWhiteSpace(result.Message)
                    ? string.Empty
                    : $"; {result.Message}";
                Debug.Log(
                    $"{LogPrefix} {result.FullName}: " +
                    $"{result.TestStatus}{details}");
            }
        }
    }
}

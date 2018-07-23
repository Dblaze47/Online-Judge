﻿using System;
using OnlineJudge.Controllers;
using OnlineJudge.Models;
using JudgeCodeRunner;
using OnlineJudge.ResponseModels;


namespace OnlineJudge.Services {
    public class JudgeService {
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void judge(SubmissionRequestData submissison){
            int problem_code = Int32.Parse(submissison.ProblemCode);
            logger.Info(String.Format("Submission recieved from user {0}, for Problem {1}", 1, problem_code));
            
            var ctx = new OjDBContext();
            var problem = ctx.Problems.Find(problem_code);

            var runner = new JudgeCodeRunner.CodeRunner(ProgrammingLanguageEnum.Cpp89,
                                                        submissison.SolutionCode,
                                                        problem.TestCaseInput,
                                                        problem.TestCaseOutput,
                                                        problem.TimeLimit);

            var submission = new Submission()
            {
                Status = ctx.SubmissionStatus.Find(Verdict.Running),
                Problem = problem,
                SourceCode = submissison.SolutionCode,
                SubmissionDate = DateTime.Now
            };

            ctx.Submissions.Add(submission);
            ctx.SaveChanges();
                

            runner.OnExecutionFinished += (sender, e) =>{
                logger.Info("Execution finished");

                var result = e.ExecutionResult;
                var status_id = result.Verdict;
                submission.Status = ctx.SubmissionStatus.Find(status_id);

                ctx.SaveChanges();
            };

            runner.RunCode();

        }

        
    }
}
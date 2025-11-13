using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QuizHandlerAutoPopulate_NewSchema : MonoBehaviour
{
    public QuizHandler target; // assign your QuizHandler in Inspector

    void Awake()
    {
        Debug.Log("QuizHandlerAutoPopulate_NewSchema: Starting initialization...");
        if (target == null) target = GetComponent<QuizHandler>();
        if (target == null) { Debug.LogError("Assign a QuizHandler to QuizHandlerAutoPopulate_NewSchema."); return; }
        target.gradesQuiz = new List<Grade>();

        Debug.Log("QuizHandlerAutoPopulate_NewSchema: Initializing Grade 4...");


        // ---- Grade 4 ----
        {
            var gradeObj = new Grade { grade = 4, levels = new List<Level>(250) };
            var subjects = QuizData_G4.SUBJECTS;
            var banks = QuizData_G4.BANKS;
            
            for (int i = 0; i < 250; i++)
            {
                var level = new Level { levelNumber = i + 1, questions = new List<Question>(subjects.Length) };
                for (int s = 0; s < subjects.Length; s++)
                {
                    var bank = banks[s];
                    if (i < bank.Length)
                    {
                        if (ParseLine(bank[i], out var qText, out var opts, out var right))
                        {
                            level.questions.Add(new Question { question = qText, answer = opts, rightAnswer = right });
                        }
                    }
                }
                gradeObj.levels.Add(level);
            }
            target.gradesQuiz.Add(gradeObj);
        }

        // ---- Grade 5 ----
        {
            var gradeObj = new Grade { grade = 5, levels = new List<Level>(250) };
            var subjects = QuizData_G5.SUBJECTS;
            var banks = QuizData_G5.BANKS;
            for (int i = 0; i < 250; i++)
            {
                var level = new Level { levelNumber = i + 1, questions = new List<Question>(subjects.Length) };
                for (int s = 0; s < subjects.Length; s++)
                {
                    var bank = banks[s];
                    if (i < bank.Length)
                    {
                        if (ParseLine(bank[i], out var qText, out var opts, out var right))
                        {
                            level.questions.Add(new Question { question = qText, answer = opts, rightAnswer = right });
                        }
                    }
                }
                gradeObj.levels.Add(level);
            }
            target.gradesQuiz.Add(gradeObj);
        }

        // ---- Grade 6 ----
        {
            var gradeObj = new Grade { grade = 6, levels = new List<Level>(250) };
            var subjects = QuizData_G6.SUBJECTS;
            var banks = QuizData_G6.BANKS;
            for (int i = 0; i < 250; i++)
            {
                var level = new Level { levelNumber = i + 1, questions = new List<Question>(subjects.Length) };
                for (int s = 0; s < subjects.Length; s++)
                {
                    var bank = banks[s];
                    if (i < bank.Length)
                    {
                        if (ParseLine(bank[i], out var qText, out var opts, out var right))
                        {
                            level.questions.Add(new Question { question = qText, answer = opts, rightAnswer = right });
                        }
                    }
                }
                gradeObj.levels.Add(level);
            }
            target.gradesQuiz.Add(gradeObj);
        }

        // ---- Grade 7 ----
        {
            var gradeObj = new Grade { grade = 7, levels = new List<Level>(250) };
            var subjects = QuizData_G7.SUBJECTS;
            var banks = QuizData_G7.BANKS;
            for (int i = 0; i < 250; i++)
            {
                var level = new Level { levelNumber = i + 1, questions = new List<Question>(subjects.Length) };
                for (int s = 0; s < subjects.Length; s++)
                {
                    var bank = banks[s];
                    if (i < bank.Length)
                    {
                        if (ParseLine(bank[i], out var qText, out var opts, out var right))
                        {
                            level.questions.Add(new Question { question = qText, answer = opts, rightAnswer = right });
                        }
                    }
                }
                gradeObj.levels.Add(level);
            }
            target.gradesQuiz.Add(gradeObj);
        }

        // ---- Grade 8 ----
        {
            var gradeObj = new Grade { grade = 8, levels = new List<Level>(250) };
            var subjects = QuizData_G8.SUBJECTS;
            var banks = QuizData_G8.BANKS;
            for (int i = 0; i < 250; i++)
            {
                var level = new Level { levelNumber = i + 1, questions = new List<Question>(subjects.Length) };
                for (int s = 0; s < subjects.Length; s++)
                {
                    var bank = banks[s];
                    if (i < bank.Length)
                    {
                        if (ParseLine(bank[i], out var qText, out var opts, out var right))
                        {
                            level.questions.Add(new Question { question = qText, answer = opts, rightAnswer = right });
                        }
                    }
                }
                gradeObj.levels.Add(level);
            }
            target.gradesQuiz.Add(gradeObj);
        }

        // ---- Grade 9 ----
        {
            var gradeObj = new Grade { grade = 9, levels = new List<Level>(250) };
            var subjects = QuizData_G9.SUBJECTS;
            var banks = QuizData_G9.BANKS;
            for (int i = 0; i < 250; i++)
            {
                var level = new Level { levelNumber = i + 1, questions = new List<Question>(subjects.Length) };
                for (int s = 0; s < subjects.Length; s++)
                {
                    var bank = banks[s];
                    if (i < bank.Length)
                    {
                        if (ParseLine(bank[i], out var qText, out var opts, out var right))
                        {
                            level.questions.Add(new Question { question = qText, answer = opts, rightAnswer = right });
                        }
                    }
                }
                gradeObj.levels.Add(level);
            }
            target.gradesQuiz.Add(gradeObj);
        }

        // ---- Grade 10 ----
        {
            var gradeObj = new Grade { grade = 10, levels = new List<Level>(250) };
            var subjects = QuizData_G10.SUBJECTS;
            var banks = QuizData_G10.BANKS;
            for (int i = 0; i < 250; i++)
            {
                var level = new Level { levelNumber = i + 1, questions = new List<Question>(subjects.Length) };
                for (int s = 0; s < subjects.Length; s++)
                {
                    var bank = banks[s];
                    if (i < bank.Length)
                    {
                        if (ParseLine(bank[i], out var qText, out var opts, out var right))
                        {
                            level.questions.Add(new Question { question = qText, answer = opts, rightAnswer = right });
                        }
                    }
                }
                gradeObj.levels.Add(level);
            }
            target.gradesQuiz.Add(gradeObj);
        }

        // ---- Grade 11 ----
        {
            var gradeObj = new Grade { grade = 11, levels = new List<Level>(250) };
            var subjects = QuizData_G11.SUBJECTS;
            var banks = QuizData_G11.BANKS;
            for (int i = 0; i < 250; i++)
            {
                var level = new Level { levelNumber = i + 1, questions = new List<Question>(subjects.Length) };
                for (int s = 0; s < subjects.Length; s++)
                {
                    var bank = banks[s];
                    if (i < bank.Length)
                    {
                        if (ParseLine(bank[i], out var qText, out var opts, out var right))
                        {
                            level.questions.Add(new Question { question = qText, answer = opts, rightAnswer = right });
                        }
                    }
                }
                gradeObj.levels.Add(level);
            }
            target.gradesQuiz.Add(gradeObj);
        }

        Debug.Log($"QuizHandlerAutoPopulate_NewSchema: Loaded {target.gradesQuiz.Count} grades successfully!");
    }

    private static bool ParseLine(string raw, out string question, out List<string> options, out string rightAnswer)
    {
        question = ""; rightAnswer = ""; options = new List<string>(4);
        if (string.IsNullOrEmpty(raw)) return false;
        // Parse "Q@@A||B||C||D@@C"
        var parts = raw.Split(new string[] { "@@" }, System.StringSplitOptions.None);
        if (parts.Length != 3) return false;
        question = parts[0].Trim();
        var opts = parts[1].Split(new string[] { "||" }, System.StringSplitOptions.None).Select(s => s.Trim()).ToList();
        if (opts.Count != 4) return false;
        string letter = parts[2].Trim().ToUpperInvariant();
        int idx = LetterToIndex(letter);
        if (idx < 0 || idx > 3) return false;
        rightAnswer = opts[idx];
        options = opts;
        return true;
    }

    private static int LetterToIndex(string letter)
    {
        switch(letter)
        {
            case "A": return 0;
            case "B": return 1;
            case "C": return 2;
            case "D": return 3;
            default: return -1;
        }
    }
}

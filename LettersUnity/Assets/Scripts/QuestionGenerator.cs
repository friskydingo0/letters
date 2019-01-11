using UnityEngine;
using System.Collections;

public enum GameOverCondition
{
	TimeUp,
	WrongAnswer,
}

public class Question
{
	public string QuestionString;
	public string LeftLetter;
	public string RightLetter;

	public Question (string questionString, string leftLetter, string rightLetter)
	{
		this.QuestionString = questionString;
		this.LeftLetter = leftLetter;
		this.RightLetter = rightLetter;
	}
}

[System.Serializable]
public class Character
{
	public char Symbol;
	public int LeftLimit;
	public int RightLimit;
}

public class QuestionGenerator : MonoBehaviour
{
	public delegate void GameStartHandler();
	public GameStartHandler OnGameStart;

	public delegate void GameOverHandler(GameOverCondition condition);
	public GameOverHandler OnGameOver;

	public delegate void QuestionGenerationHandler(Question qString);
	public QuestionGenerationHandler OnQuestionGenerated;

	public Character[] _CharacterSet;
	public char[] _LetterSet;

	public char pSelectedLetter
	{
		get;
		private set;
	}

	public char pAnswer
	{
		get;
		private set;
	}

	public int _DifferenceRange = 0;
	public int _MaxDifferenceRange = 0;

	private char mLeftChoice;
	private char mRightChoice;

	public float _TimePerQuestion = 3;
	private float mQuestionTimer = 0;

	public float pQuestionTimer
	{ get{ return (mQuestionTimer/_TimePerQuestion); } }

	private bool mInitComplete = false;

	// TODO : Remove this and put it where it meant to be handled. Possibly in score manager??
	public UnityEngine.UI.Text _TimerText;
	public UnityEngine.UI.Text _AverageTimerText;

	private float mAverageAnswerTime = 0;
	private float mTotalAnswerTime = 0;
	private float mQuestionCount = 0;

	void Start()
	{
		// Init character set
		_LetterSet = new char[26];

		for(int charIndex = 0; charIndex < 26; charIndex++)
		{
			_LetterSet[charIndex] = (char)(charIndex + (int)('A'));
		}
	}

	public void StartGame()
	{
		GenerateNextQuestion();

		if(OnGameStart != null)
			OnGameStart();

		mInitComplete = true;
	}

	public void ResetGame()
	{
		mAverageAnswerTime = 0;
		mTotalAnswerTime = 0;
		mQuestionCount = 0;

		_TimerText.text = "0";
		_AverageTimerText.text = "0";
	}

	private void GenerateNextQuestion()
	{
		// Select a letter
		int index = Random.Range(0,_LetterSet.Length);
		pSelectedLetter = _LetterSet[index];

//		Debug.Log("Selected letter : " + pSelectedLetter);

		// Get valid range for selected letter
		// index - _DRange <= 0 -> Left end of spectrum
		// index + _Drange >= length -> Right end of spectrum
		// If on left end, leftLimit should be 0
		// If on right end, rightLimit should be Length

		bool isOnLeft = (index - _DifferenceRange <= 0);
		bool isOnRight = (index + _DifferenceRange >= _LetterSet.Length);

//		Debug.Log("On left : " + isOnLeft);
//		Debug.Log("On right : " + isOnRight);

		// Select a letter difference - is for left and + is for right
		int difference = 0;
		int leftLimit = 0;
		int rightLimit = 0;

		if(isOnLeft)
			leftLimit = 0;
		else
			leftLimit = -_DifferenceRange;

		if(isOnRight)
			rightLimit = 0;
		else
			rightLimit = _DifferenceRange;

		do
		{
			difference = Random.Range(leftLimit, rightLimit);
//			Debug.Log("Index : " + index + " | Difference : " + difference + " | Choice : " + (index + difference));

//			if(char.IsUpper((char) ((int)(pSelectedLetter) + difference)))
//				Debug.Log("Is Letter");
//			else
//				Debug.Log("Is Not Letter");
		}
		while(difference == 0);
//		Debug.Log("Selected Difference : " + difference);

		// Assign answer based on selected difference
		pAnswer = (char)((int)(pSelectedLetter) + difference);
//		Debug.Log("Answer : " + pAnswer);

		string spaces = " ";
		for(int selectedIndex = 0; selectedIndex < Mathf.Abs(difference) - 1; selectedIndex++)
		{
			spaces += " _ ";
		}

		bool isLeftCorrect = (Random.Range(0, 2) == 0 ? true : false);

		if(isLeftCorrect)
		{
			mLeftChoice = pAnswer;
			mRightChoice = GenerateFakeLetter();
		}
		else
		{
			mLeftChoice = GenerateFakeLetter();
			mRightChoice = pAnswer;
		}

		string questionString = "";
		if(pSelectedLetter > pAnswer)
		{
			questionString = "?"+ spaces + pSelectedLetter;
//			Debug.Log("?"+ spaces + pSelectedLetter);
		}
		else
		{
			questionString = pSelectedLetter + spaces + "?";
//			Debug.Log(pSelectedLetter + spaces + "?");
		}
		Question newQuestion = new Question(questionString, mLeftChoice.ToString(), mRightChoice.ToString());

		if(OnQuestionGenerated != null)
			OnQuestionGenerated(newQuestion);

		mQuestionTimer = 0;
		mQuestionCount++;

		if(mQuestionCount % 5 == 0)
		{
			int range = _DifferenceRange + 1;//(int)(mQuestionCount/5);
			_DifferenceRange = (int)Mathf.Clamp(range, 0, _MaxDifferenceRange);
		}
	}

	/// <summary>
	/// Generates the fake letter which will be assigned to one of the choices.
	/// </summary>
	/// <returns>The fake letter.</returns>
	private char GenerateFakeLetter()
	{
		char fakeLetter = '\0';
		do
		{
			fakeLetter = _LetterSet[Random.Range(0, _LetterSet.Length)];
		}
		while(fakeLetter == pAnswer);

		return fakeLetter;
	}

	public void CheckAnswer(bool isLeftButtonClicked)
	{
		char choice = mLeftChoice;
		if(!isLeftButtonClicked)
			choice = mRightChoice;

		mTotalAnswerTime += mQuestionTimer;
		mAverageAnswerTime = mTotalAnswerTime / mQuestionCount;

//		Debug.Log("Time taken : " + mQuestionTimer);
		_TimerText.text = mQuestionTimer.ToString();

		if(choice == pAnswer)
		{
			GenerateNextQuestion();
		}
		else
		{
			// Remove this. ONLY FOR TESTING
//			GenerateNextQuestion();
			GameOver(GameOverCondition.WrongAnswer);
		}

		_AverageTimerText.text = mAverageAnswerTime.ToString();
	}

	void Update()
	{
		if(mInitComplete)
		{
			if(mQuestionTimer >= _TimePerQuestion)
			{
				// Game over
				GameOver(GameOverCondition.TimeUp);

				mInitComplete = false;
				mQuestionTimer = 0;
			}
			else
			{
				mQuestionTimer += Time.deltaTime;
			}
		}
	}

	private void GameOver(GameOverCondition failCondition)
	{
		if(OnGameOver != null)
			OnGameOver(failCondition);
	}
}

using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public QuestionGenerator _QGen;

	public Image _Timer;

	public Text _QuestionText;
	public Text _AnswerLeft;
	public Text _AnswerRight;

	public Text _Result;

	public GameObject _Tint;

	private bool mIsGameOver = false;

	void Start()
	{
		_QGen.OnGameStart += OnGameStarted;
		_QGen.OnQuestionGenerated += OnQuestionGenerated;
		_QGen.OnGameOver += OnWrongAnswer;

		StartCoroutine(StartAfterSeconds(5));
	}

	IEnumerator StartAfterSeconds(float timeToWait)
	{
		mIsGameOver = false;

		yield return new WaitForSeconds(timeToWait);
		_Result.text = "Start NOW!";
		_QGen.StartGame();
	}

	private void OnGameStarted()
	{
		_Tint.SetActive(false);
	}

	void Update()
	{
		if(mIsGameOver)
		{
			if(Input.GetMouseButtonDown(0))
			{
				_QGen.ResetGame();
				StartCoroutine(StartAfterSeconds(1));
			}
		}
		else
		{
			_Timer.fillAmount = _QGen.pQuestionTimer;
		}
//		else if(Input.GetKeyDown(KeyCode.Escape))
//		{
//			Application.Quit();
//		}
	}

	public void OnClickAnswer(bool isLeftButton)
	{
		_QGen.CheckAnswer(isLeftButton);
	}

	private void OnQuestionGenerated(Question newQuestion)
	{
		_Result.text = "Right answer!";

		_QuestionText.text = newQuestion.QuestionString;
		_AnswerLeft.text = newQuestion.LeftLetter;
		_AnswerRight.text = newQuestion.RightLetter;
	}

	private void OnWrongAnswer(GameOverCondition condition)
	{
		mIsGameOver = true;
		_Tint.SetActive(true);

		if(condition == GameOverCondition.WrongAnswer)
		{
			_Result.text = "Game over. Wrong answer! Tap to restart.";
		}
		else
			_Result.text = "Time UP!";
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Menu : MonoBehaviour {

	[Header("Menu Settings")]
	public bool active;																		// Responsible variable if the button is active or not

	public int maxFontSize = 22;															// Maximum font size
	public int minFontSize = 18;															// Minimum  font size

	public Color mouseEnter;																// MenuItem color when the mouse enters
	public Color mouseExit;																	// MenuItem color when the mouse exits
	public Color mousePressed;																// MenuItem color when the mouse pressed / click
	public Color deactivatedColor;															// MenuItem color when disabled

	[Header("Line Settings")]
	public bool enableLine;																	// Enable underline
	public bool enableLineEffect;															// Enable effect when mouse enters
	public float widthLine=200f;															// Width underline
	public float heightLineMin=1f;															// Minimum height underline
	public float heightLineMax=2f;															// Maximum height underline

	[Header("Animation Settings")]
	public bool initAnim;																	// Initial animation
	public float timerInitAnim;																// Time for animation starts

	public float menuXStart = -233f;														// Initial X-axis
	public float menuXEnd = 115f;															// End X-axis
	public float speedAnim = 400f;															// Speed animation

	// Variables that the user does not need to change
	private MenuControl _menuc;															// Menu Control Component
	private Text _text;																	// Text Component
	private Vector3 _initPos;															// Initial position
	private RectTransform _rect;	

	AudioManager audioManager;
	Player player;

	// Use this for initialization
	void Start () {
		getComponents ();
		basicSettings ();
		player = GameObject.FindGameObjectWithTag ("Player").GetComponent<Player>();
		audioManager = GameObject.FindGameObjectWithTag ("AudioManager").GetComponent<AudioManager>();

			
	}// END
	
	// Update is called once per frame
	void Update () {
		if (initAnim == true) {
			updateAnimation ();
		}
	}// END

	//-----------------------------------------------------------------------------START METHODS MENUITEM--------------------------------------------------------------------\\
	// Get the components
	void getComponents(){
		_rect = this.GetComponent<RectTransform> ();										// Get the RectTransform component of this object
		_menuc = FindObjectOfType<MenuControl> ();											// Get the Control Menu (there should only be one)
		_text = gameObject.GetComponentInChildren<Text> ();                                                 // Get the Text component of children
    }// END

	// Basic and necessary settings
	void basicSettings(){
		_initPos = _rect.localPosition;														// Get initial position
		if (initAnim == true) {																// If the initial animation is true
			_rect.localPosition = new Vector3(menuXStart, _initPos.y, _initPos.z);			// Arrow the position of the object to the X axis of the variable "menuXStart"
		}

		//Set Default Color
		_text.color = mouseExit;

		// If the button is not active
		if (active == false) {
			_text.color = deactivatedColor; // Set color to "deactivatedColor"
		}

	}// END

	// Update initial animation
	void updateAnimation(){
		// If the time to start the animation is over
		if (timerInitAnim <= 0) {
			_rect.transform.localPosition = Vector2.MoveTowards (_rect.transform.localPosition, new Vector2 (menuXEnd, _initPos.y), speedAnim * Time.deltaTime); // Starts Animation
		}
		if (timerInitAnim >= 0) {timerInitAnim -= Time.deltaTime;} // If the animation time is greater than zero (ie not started) it starts to go down 1 second
	}// END
	//-----------------------------------------------------------------------------END METHODS MENUITEM----------------------------------------------------------------------\\


	//-----------------------------------------------------------------------------START METHODS ON/OFF----------------------------------------------------------------------\\
	// Method by setting the default menuItem again (mouseExit)
	public void menuDisable(){
		if (active == true) {																			// If the button is active
			_text.color = mouseExit;																		// Sets the default color															// Sets the default color (underline)
			_text.fontSize = minFontSize;
			if (player != null) {
				player.inputOnButtonPress = true;
			}
		}
	}// END

	// Method by setting the active menuItem (mouseEnter)
	public void menuEnable(){
		if (active == true) {																			// If the button is active
			_text.color = mouseEnter;																	// Sets new color (mouseEnter)
			_text.fontSize = maxFontSize;
			if (player != null) {
				player.inputOnButtonPress = false;
			}
		}
	}// END

	public void PlayClickSound(){
		if (active == true) {
			audioManager.PlaySound ("MenuClick");
		}
	}

	// Activate an object and mask it
	public void enableObject(GameObject obj){
		obj.SetActive (true);																			// Active the object
		_menuc.mask.SetActive (true);																	// Active the mask
		_menuc.setAlphaMask (0.5f);																		// Set alpha of mask to 0.5f
	}// END
	//-----------------------------------------------------------------------------END METHODS ON/OFF------------------------------------------------------------------------\\

	public void showMessageInConsole(string s){
		Debug.Log (s);
	}
}

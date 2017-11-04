using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UI_ScreenInterface : MonoBehaviour {

	//variables
		//element style
		private GUIStyle convoStyle = null;
		private GUIStyle pauseStyle = null;

		//element hide
		private bool convoDisplay = false;
		private bool pauseDisplay = false;

        public bool Paused
        {
            get
            {
                return pauseDisplay;
            }
        }

		//element dimension: params:= xpos,ypos,width,height
		private Rect convoDim = new Rect(0.05f*Screen.width,0.75f*Screen.height,0.6f*Screen.width,
			0.2f*Screen.height);
		private Rect pauseDim = new Rect(0.45f*Screen.width,0.45f*Screen.height,0.1f*Screen.width,
			0.1f*Screen.height);

		//misc info
		private LinkedList<string> convoText = new LinkedList<string>();
		private int textBreak = -1;  //indicates character position of where text clips off of textbox

		//Speed for pause menu
		private float lastSpeed = 0;

    //methods
    void Start()
    {

        convoDim = new Rect(0.05f * Screen.width, 0.75f * Screen.height, 0.6f * Screen.width,
            0.2f * Screen.height);
        pauseDim = new Rect(0.45f * Screen.width, 0.45f * Screen.height, 0.1f * Screen.width,
            0.1f * Screen.height);

    }

    void Update(){
		//User Input checks
		if(Input.GetKeyDown(KeyCode.P)){
			if(!pauseDisplay){
				lastSpeed = Time.timeScale;
				Time.timeScale = 0;
				pauseDisplay = true;
			}

			else{
				Time.timeScale = lastSpeed;
				lastSpeed = 0;
				pauseDisplay = false;
			}
		}

        //ignore input until unpaused
        else if (pauseDisplay)
            return;

        //Proceed through dialogue
        else if (Input.GetKeyDown(KeyCode.Space) && convoText.Count > 0)
        {
            proceedText();
        }
	}

	//Dialogue Box Methods
		//states whether all dialogue text has been seen
		public bool IsTextRead(){
			return !convoDisplay;
		}

		//states whether dialogue text is vertically clipped
		private bool IsConvoLong(){
			return (convoDim.height < convoStyle.CalcHeight(new GUIContent(convoText.First.Value),convoDim.width));
		}

		//determines at which point text is vertically clipped
		private void FindTextBreak(){

			//initialization and constants
			textBreak = 0;
			int actLen = convoText.First.Value.Length;

        //determine the breakpoint in text
			while(	(textBreak < actLen) && (convoDim.height > convoStyle.CalcHeight(
				new GUIContent(convoText.First.Value.Substring(0,textBreak)),convoDim.width))){
				textBreak++;
			}

            //ensure line of cut text isn't included
            textBreak--;

        //move back break to show complete word

            if (textBreak < actLen-1)
            {
                while (convoText.First.Value[textBreak] != ' ' && convoText.First.Value[textBreak] != '\t' &&
                    convoText.First.Value[textBreak] != '\n')
                {
                    textBreak--;
                }
            }

            //increment to allow all characters before textBreak to be printed
			textBreak++;
		}

		//receive dialogue
		public void DeliverDialogue( string[] dialogue ){

			foreach( string str in dialogue ){
			convoText.AddLast(new LinkedListNode<string>(str));
			}

			convoDisplay = true;
        
		}

        private void proceedText(){
            //show remainder of clipped text
            if (IsConvoLong()){
                convoText.First.Value = convoText.First.Value.Substring(textBreak,
                convoText.First.Value.Length-textBreak);

                textBreak = convoText.First.Value.Length;

                if(IsConvoLong())
                    FindTextBreak();
            }

            //show next set of dialogue
            else{
                convoText.RemoveFirst();

                //prevent invalid access to text that doesn't exist
                if (convoText.Count <= 0){
                        convoDisplay = false;
                }
                else{
                    textBreak = convoText.First.Value.Length;

                    if (IsConvoLong())
                        FindTextBreak();
                }
             }

        }

    //GUI methods
        private void guiInit(){

			//dialogue box style
				if(convoStyle == null){
					convoStyle = new GUIStyle( GUI.skin.box );
					convoStyle.normal.background = MakeTex(2,2,new Color(1,0,0,0.2f));
					convoStyle.alignment = TextAnchor.UpperLeft;
					convoStyle.fontStyle = FontStyle.Bold;
					convoStyle.wordWrap = true;
				}

			//Pause Text style
				if(pauseStyle == null){
					pauseStyle = new GUIStyle(GUI.skin.textField);
					pauseStyle.alignment = TextAnchor.MiddleCenter;
				}

		}

		void OnGUI(){

			//initialization
				guiInit();

            //Dialogue display
				if(convoDisplay){
                    if (textBreak == -1)
                        FindTextBreak();
					GUI.Box( convoDim, convoText.First.Value.Substring(0,textBreak), convoStyle );
				}

			//Pause display
				if(pauseDisplay){
					GUI.Box( pauseDim, "Pause", pauseStyle);
				}
		
		}

		//creates image with respect to parameter specifications
		private Texture2D MakeTex( int width, int height, Color col){

			//create texture of width*height size of color col
				Color[] pix = new Color[width * height];
				for( int i = 0; i < pix.Length; ++i){
					pix[i] = col;
				}
				Texture2D result = new Texture2D(width,height);
				result.SetPixels(pix);
				result.Apply();
				return result;
		}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Print{

	public static void Log(object message){
		
		if( Constants.PRINT_DEBUG ){
			Debug.Log(message);
		}
	}
}
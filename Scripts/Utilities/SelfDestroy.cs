using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
	private void Awake()
	{
		Destroy(gameObject);
	}
}

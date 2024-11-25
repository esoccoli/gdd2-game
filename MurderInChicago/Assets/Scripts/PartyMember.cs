using System.Collections;

public class PartyMember : Character
{
    /// <summary>
    /// Waits for input from the UI and takes the appropriate action based on the specific input
    /// </summary>
    /// <returns></returns>
    public IEnumerator AwaitInputFromUI()
    {
        //yield return new WaitForSeconds(2.0f);
        while (IsMyTurn)
        {
            yield return null;
        }
    }
}

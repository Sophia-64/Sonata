Xontrols adds a few useful classes designed for dealing with state machines. They are as follows:

BControl: A powerful and dirt simple boolean map that returns preset value (true or false) only if all of the booleans in the map match. 
It's a bit like the boolean equivalent of a jury. 
Syntax:
```
BControl blockPlayerCtrl = false; //Declares that the player should only have control if all conditions agree

blockPlayerCtrl["bear trap"]       = true  ; //Sets a bool to the key "bear trap", or creats it if it does not exist
blockPlayerCtrl[this.transform]    = false ; //component / monobehavior control, makes a component key. 
blockPlayerCtrl[InventoryItemUI]   = false ; //object controls, for any non-data type
blockPlayerCtrl[666]               = true  ; //Integer controls, too, and they work as identifier, not indices.

blockPlayerCtrl.OnBoolAssigned += SomeFunction; //Adds a listener to an event called any time a boolean member is changed

if (blockPlayerCtrl.stringControls.ContainsKey("yo")) ... //Checks for a specific key without creating one
```

The BControl is useful anywhere a lot of different flags need to decide a single branch (state machines, e.g). 

Example
```
public static class Courtroom
{
  //If I set 'isGuilty' = true, it will only be true if all bools are in agreement. 
  public static BControl isGuilty = true; 
  public Meatbag defendant;
  
  void CheckVerdict()
  {
      if (isGuilty) defendant.ExileToAustralia(); //BControl is read like a normal boolean.
  }

  void StartTrial()
  {
      isGuilty.OnBoolAssigned += CheckVerdict(); //Bcontrols offer the option to run a delegate if any member bool is added or modified
  }
}

//If one juror says you're innocent, you're innocent. If there are no jurors, only the judge can save you.
public class Juror : Meatbag
{
  void JurorVerdict(int ID)
  {
      if (this_juror_thinks_you_are_innocent)
      {
          RaiseHand();
          Courtroom.isGuilty[ID] = false; //Activating a key creates it if its' new, sets it if it isn't. That's it
                                          //The BControl runs on dictionaries, so pick any int you want.
      }
      else Courtroom.isGuilty[ID] = true; 

      if (jurorIsInfestedByAlienParasite) Courtroom.isGuilty["alien infestation"] = true;
  }
}

//Meanwhile, on some object, somewhere, on its own time
public class Judge : Meatbag
{
    void CalledMenInBlack()
    {
      Courtroom.isGuilty["alien infestation"] = false;  //aliens think you are innocent and override the jury
    }
  
    void ActivateNepotism()
    {
      Courtroom.isGuilty[this] = false;  //judge thinks you're nice and overrides the jury
    }
}

```
--

Let's consider what this would look like without a BControl. 

--
```
  //racing conditions, or complex flag checks are necessary
  //a smart programmer will make a dictionary of bools instead
  //BControl does all that for you.

  static bool isGuilty = true ; 
  public Meatbag defendant    ;
  static Judge judge          ; //need to track a reference when a new judge takes the shift
  static List<Juror>  Jurors  ; //need add jurors to this list when they are created and loop through
  static alienInfestation     ; //need the aliens to let us know not to convict you 
  static nepotism             ; //need the judge to tell us not to convict you if they're your buddy
  
void CheckVerdict()
{ 
  //need to loop through jurors, or smarter handling of flags
  //fine if there are 12, but what if you're being judged by a panel of 9 septillion mosquitos? 
  foreach (var juror in Jurors)                                     
    if (juror.this_juror_thinks_you_are_innocent) isGuilty = false;
    
  if (alienInfestation || nepotism) isGuilty = false;

  if (isGuilty) defendant.ExileToAustralia();
}

//...and so on
--
```
This leads to a lot of racing conditions and a lack of independence for various mechanics. 
The BControl's performance is not significantly different than this scenario. But in my opinion,
it is lot more intuitive. 


-----
IControl: 

The indexing / lookup system is identical to BControl. The difference is that instead of a boolean, it operates on an integer, and it returns the minimum or maximum value depending on the mode set.
It is useful for having many different, independent objects cleanly operate on the same integer. 

Unlike BControl, IControl does not have an event upon changing an integer. Feel free to add one.

Syntax
```
IControl ControlLevel = IControlMode.Maximum;  //It will return a maximum of the values. 

ControlLevel["menu open"]       = 3; //The menu being open suggests the control should be 3
ControlLevel[enemy]             = 1; //an enemy wants control level to be 1
ControlLevel[34]                = 0; //some weird OS condition says it should be 0
ControlLevel[player.rigidbody]  = 4; //According to the player's rigidbody, we're going fast, let's make it 4

Debug.Log($"CTRL Level = {ControlLevel}); // Spits out "CTRL Level = 4", because the highest number set is 4
```

XControl

This is the most advanced of the set.  It is a generic, XControl<T>, designed for handling superposed complex states. 

Like a BControl, it is keyhed with strings, ints, components, and objects. But the value returned is not one value, but a 
tuple of three values -- a condition, an integer, and an object of type T. The returning system works the same as a BControl.

It is most useful for handling complicated conditions, e.g knowing what description to set on a highlighting box when there may be 
multiple overlapping objects. It is powerful and flexible, and enables scripts to be more self-contained.


Syntax:
```
XControl<string> HighlightMap = false; //HighlightMap as a boolean returns 'false' unless any member has a boolean component of 'true'. 

HighlightMap[SomeUIElement] = (true, 1, "label 1")  ; //According to SomeUIElement, it should be highlighted, it's priority 1, and the label should say "label 1". 
HighlightMap["Bad Wolf"]    = (false, 99, "doctor") ; //According to "Bad Wolf", it shouldn't be highlighted, but it'd be top priority if it were
HighlightMap[666]           = (true, 0, "oh my")    ; //According to something identified as the number 666, it should be highlighted with the label "oh my"
HighlightMap[someRefType]   = (true, 4, "rosebud")  ; //According to this object, it should be highlighted with the label "rosebud". 
```

//Now, let's say that you have a highlighting utility. Should the following be true...
```
...{
      (bool showHL, string label) HLData = HighlightMap;
    
      if (showHL) 
      {
        HighlightBox.SetActive(true); //show the UI
        HighlightBox.SetText(label); //show the label with the highest priority
      }
    }
```
In this case, the label would be "rosebud", because it's the highest order option with the lowest condition. 

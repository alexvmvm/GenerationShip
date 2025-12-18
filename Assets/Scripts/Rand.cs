using UnityEngine;

public class Rand
{
    public static bool Bool => Chance(0.5f);
    public static float Value => Random.Range(0, 1f);

    public static bool Chance(float chance)
    {
        return Random.Range(0f, 1f) < chance;
    }

    public static int Range(int min, int max)
    {
        return Random.Range(min, max);
    }

    public static float Range(float min, float max)
    {
        return Random.Range(min, max);
    }

    public static bool MTBEventOccurs(float mtb, float mtbUnit = 1, float checkDuration = 1)
    {
        if( mtb == float.PositiveInfinity )
        {
            //This does legitimately happen in some cases
            return false;
        }

        if( mtb <= 0 )
        {
            Debug.LogError("MTBEventOccurs with mtb=" + mtb);
            return true;
        }

        if( mtbUnit <= 0 )
        {
            Debug.LogError("MTBEventOccurs with mtbUnit=" + mtbUnit);
            return false;
        }

        if( checkDuration <= 0 )
        {
            Debug.LogError("MTBEventOccurs with checkDuration=" + checkDuration);
            return false;
        }

        //Rapid version
        double chancePerCheck = (double)checkDuration/((double)mtb*mtbUnit);
        
        
        if( chancePerCheck <= 0 )
        {
            //This should never happen but causes crashes so we need to stop it if it does
            Debug.LogError("chancePerCheck is " + chancePerCheck + ". mtb=" + mtb + ", mtbUnit=" + mtbUnit + ", checkDuration=" + checkDuration);
            return false;
        }

        //This solves float precision issues by splitting one Rand.Value check into two
        double otherChance = 1d;
        if( chancePerCheck < 0.0001d )
        {
            while( chancePerCheck < 0.0001d )
            {
                chancePerCheck *= 8;
                otherChance /= 8;
            }
        
            if( Value > otherChance )
                return false;
        }

        return Value < chancePerCheck;
    }
}
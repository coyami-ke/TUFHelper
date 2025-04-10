using Newtonsoft.Json;
using System.Linq.Expressions;

namespace TUFHelper.Utils
{
    public class PassInfo
    {

        public int id;
        public float accuracy;
        public string player, speed, vidLink, levelId, vidUploadTime, scoreV2;
        public string[] judgements;

        [JsonConstructor]
        public PassInfo() {
            
        }

        public double GetScoreV2()
        {
            try
            {
                return double.Parse(scoreV2);
            } 
            catch
            {
                return 0; 
            }
         
        }

        public int[] GetJudgements()
        {
            try
            {
                int[] judge = new int[6];
                for (int i = 0; i < 6; i++)
                {
                    judge[i] = int.Parse(judgements[i]);
                }
                return judge;
            }
            catch
            {
                return new int[] { 0, 0, 0, 0, 0, 0 };
            }
        }

        public double getXAcc()
        {
            return accuracy;
        }

        public int GetLevelId()
        {
            if (levelId == null)
            {
                return 0;
            }
            else
            {
                return int.Parse(levelId);
            }
        }

        public float GetSpeed()
        {
            return speed == null ? 1.0f : float.Parse(speed);
        }
    }

}
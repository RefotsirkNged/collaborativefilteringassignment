namespace MovieAnalysis
{
    class Review
    {
        int userID;
        int movieID;
        int score;
        private int extraInfo;

        public Review(int userID, int movieID, int score, int extraInfo)
        {
            UserID = userID;
            MovieID = movieID;
            Score = score;
            ExtraInfo = extraInfo;
        }

        public int UserID { get => userID; set => userID = value; }
        public int MovieID { get => movieID; set => movieID = value; }
        public int Score { get => score; set => score = value; }
        public int ExtraInfo { get =>  extraInfo; set => extraInfo = value; }
    }
}
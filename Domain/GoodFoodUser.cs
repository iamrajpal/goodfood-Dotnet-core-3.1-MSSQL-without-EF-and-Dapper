namespace Domain
{
    public class GoodFoodUser
    {
        public string UserName { get; set; }
        public byte[] User_Password_Hash { get; set; }
        public byte[] User_Password_Salt { get; set; }
    }
}

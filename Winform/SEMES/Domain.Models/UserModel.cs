using Domain.Models.Contracts;
using Infra.DataAccess.Contracts;
using Infra.DataAccess.Entities;
using Infra.DataAccess.Repositories;
using Infra.EmailServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class UserModel : IUserModel
    {
        /// <summary>
        /// This class implements the IUserModel interface along with its defined methods.
        /// </summary>

        #region -> Attributes

        private int _id;
        private string _username;
        private string _password;
        private string _firstName;
        private string _lastName;
        private string _position;
        private string _email;
        private byte[] _photo;
        private IUserRepository _userRepository;
        #endregion

        #region -> Constructors

        public UserModel()
        {
            _userRepository = new UserRepository();
        }

        public UserModel(int id, string userName, string password, string firstName, string lastName, string position, string email, byte[] photo)
        {
            Id = id;
            Username = userName;
            Password = password;
            FirstName = firstName;
            LastName = lastName;
            Position = position;
            Email = email;
            Photo = photo;

            _userRepository = new UserRepository();
        }
        #endregion

        #region -> Properties

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }
        public string Position
        {
            get { return _position; }
            set { _position = value; }
        }
        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }
        public byte[] Photo
        {
            get { return _photo; }
            set { _photo = value; }
        }
        #endregion

        #region -> Public Methods

        public int Add(UserModel userModel)
        {
            var userEntity = MapUserEntity(userModel);
            return _userRepository.Add(userEntity);
        }
        public int Edit(UserModel userModel)
        {
            var userEntity = MapUserEntity(userModel);
            return _userRepository.Edit(userEntity);
        }
        public int Remove(UserModel userModel)
        {
            var userEntity = MapUserEntity(userModel);
            return _userRepository.Remove(userEntity);
        }
        public int AddRange(List<UserModel> userModels)
        {
            var userEntityList = MapUserEntity(userModels);
            return _userRepository.AddRange(userEntityList);
        }
        public int RemoveRange(List<UserModel> userModels)
        {
            var userEntityList = MapUserEntity(userModels);
            return _userRepository.RemoveRange(userEntityList);
        }

        public UserModel GetSingle(string value)
        {
            var userEntity = _userRepository.GetSingle(value);
            if (userEntity != null)
                return MapUserModel(userEntity);
            else return null;
        }
        public IEnumerable<UserModel> GetAll()
        {
            var userEntityList = _userRepository.GetAll();
            return MapUserModel(userEntityList);
        }
        public IEnumerable<UserModel> GetByValue(string value)
        {
            var userEntityList = _userRepository.GetByValue(value);
            return MapUserModel(userEntityList);
        }

        public UserModel Login(string username, string password)
        {
            var userEntity = _userRepository.Login(username, password);
            if (userEntity != null)
                return MapUserModel(userEntity);
            else return null;

        }
        public UserModel RecoverPassword(string requestingUser)
        {//Method to recover the user's password and send it to their email address.
           var userModel= GetSingle(requestingUser);
           if (userModel != null)
           {
               var mailService = new EmailService();
               mailService.Send(
                   recipient: userModel.Email,
                   subject: "Password recovery request",
                   body: "Hi " + userModel.FirstName + ",\nYou requested to recover your password.\n" +
                   "Your current password is: " + userModel.Password + 
                   "\nHowever, we ask that you change your password immediately once you enter the application.");              
           }
           return userModel;
            /*Note: This is just an example for sending emails, it is not a good idea to directly send the user's password,
             * instead it is better to send a temporary password.*/
        }
        #endregion

        #region -> Private Methods (Map data)

        //Map entity model to domain model.
        private UserModel MapUserModel(User userEntity)
        {//Map a single object.
            var userModel = new UserModel
            {
                Id = userEntity.Id,
                Username = userEntity.Username,
                Password = userEntity.Password,
                FirstName = userEntity.FirstName,
                LastName = userEntity.LastName,
                Position = userEntity.Position,
                Email = userEntity.Email,
                Photo = userEntity.Photo
            };
            return userModel;

        }
        private List<UserModel> MapUserModel(IEnumerable<User> userEntityList)
        {//Map collection of objects.
            var userModelList = new List<UserModel>();

            foreach (var userEntity in userEntityList)
            {
                userModelList.Add(MapUserModel(userEntity));
            };
            return userModelList;
        }

        //Map domain model to entity model.
        private User MapUserEntity(UserModel userModel)
        {//Map a single object.
            var userEntity = new User
            {
                Id = userModel.Id,
                Username = userModel.Username,
                Password = userModel.Password,
                FirstName = userModel.FirstName,
                LastName = userModel.LastName,
                Position = userModel.Position,
                Email = userModel.Email,
                Photo = userModel.Photo
            };
            return userEntity;
        }        
        private List<User> MapUserEntity(List<UserModel> userModelList)
        {//Map a collection of users.
            var userEntityList = new List<User>();

            foreach (var userModel in userModelList)
            {
                userEntityList.Add(MapUserEntity(userModel));
            };
            return userEntityList;
        }
        #endregion
       
    }
}

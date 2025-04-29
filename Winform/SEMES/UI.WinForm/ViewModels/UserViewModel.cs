using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Domain.Models;

namespace UI.WinForm.ViewModels
{
    public class UserViewModel
    {
        #region -> Fields

        private int _id;
        private string _username;
        private string _password;
        private string _firstName;
        private string _lastName;
        private string _position;
        private string _email;
        private byte[] _photo;

        #endregion

        #region -> Constructors

        public UserViewModel()
        {

        }

        public UserViewModel(int id, string userName, string password, string firstName, string lastName, string position, string email, byte[] photo)
        {
            Id = id;
            Username = userName;
            Password = password;
            FirstName = firstName;
            LastName = lastName;
            Position = position;
            Email = email;
            Photo = photo;
        }
        #endregion

        #region -> Properties + Data Validation and Visualization.

        //Position 0 
        [DisplayName("Num")]//Display name (For example, in the datagridview column it will be shown as Num).
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        //Position 1  
        [Required(ErrorMessage = "Username is required.")]//Validations
        [StringLength(100, MinimumLength = 5, ErrorMessage = "The username must contain a minimum of 5 characters.")]
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        //Position 2      
        [Browsable(false)]//Hide display (For example do not show in the datagridview).
        [Required(ErrorMessage = "Please enter a password.")]//Validations
        [StringLength(100, MinimumLength = 5, ErrorMessage = "The password must contain a minimum of 5 characters.")]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        //Position 3
        [DisplayName("First name")]//Display name.
        [Browsable(false)]//Hide display
        [Required(ErrorMessage = "Please enter name")]
        [RegularExpression("^[a-zA-Z ]+$", ErrorMessage = "The name must be only letters")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "The name must contain a minimum of 3 characters.")]
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        //Position 4
        [DisplayName("Last name")]//Display name.
        [Browsable(false)]//Hide display
        [Required(ErrorMessage = "Please enter last name.")]//Validations
        [RegularExpression("^[a-zA-Z ]+$", ErrorMessage = "The last name must be only letters")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "The last name must contain a minimum of 3 characters.")]
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }

        //Position 5
        [ReadOnly(true)]
        [DisplayName("Full name")]//Display name.
        public string FullName
        {
            get { return _firstName + ", " + _lastName; }
        }

        //Position 6
        [Required(ErrorMessage = "Please select a position.")]//Validations
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Position must contain a minimum of 8 characters.")]
        public string Position
        {
            get { return _position; }
            set { _position = value; }
        }

        //Position 7
        [DisplayName("Email")]//Display name.
        [Required(ErrorMessage = "Please enter email.")]//Validations
        [EmailAddress(ErrorMessage = "Please enter a valid email: example@gmail.com")]
        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        //Position 8
        [DisplayName("Foto")]//Display name.
        [Browsable(false)]//Hide display
        public byte[] Photo
        {
            get { return _photo; }
            set { _photo = value; }

        }
        #endregion

        #region -> Methods (Map data)

        //Map domain model to view model
        public UserViewModel MapUserViewModel(UserModel userModel)
        {
            var userViewModel = new UserViewModel
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
            return userViewModel;
        }
        public List<UserViewModel> MapUserViewModel(IEnumerable<UserModel> userModelList)
        {
            var userViewModelList = new List<UserViewModel>();

            foreach (var userModel in userModelList)
            {
                userViewModelList.Add(MapUserViewModel(userModel));
            };
            return userViewModelList;
        }

        //Map view model to domain model
        public UserModel MapUserModel(UserViewModel userViewModel)
        {
            var userModel = new UserModel
            {
                Id = userViewModel.Id,
                Username = userViewModel.Username,
                Password = userViewModel.Password,
                FirstName = userViewModel.FirstName,
                LastName = userViewModel.LastName,
                Position = userViewModel.Position,
                Email = userViewModel.Email,
                Photo = userViewModel.Photo
            };
            return userModel;
        }        
        public List<UserModel> MapUserModel(List<UserViewModel> userViewModelList)
        {
            var userModelList = new List<UserModel>();

            foreach (var userViewModel in userViewModelList)
            {
                userModelList.Add(MapUserModel(userViewModel));
            };
            return userModelList;
        }
        #endregion
    }
}

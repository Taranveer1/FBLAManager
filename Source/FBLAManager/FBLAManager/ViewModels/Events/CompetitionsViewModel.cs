﻿using FBLAManager.Helpers;
using FBLAManager.Models;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FBLAManager.ViewModels.Events
{
    /// <summary>
    /// ViewModel for the competition page.
    /// </summary>
    public class CompetitionsViewModel : BaseViewModel
    {
       
        /// <summary>
        /// Initializes a new instance for the <see cref="T:FBLAManager.ViewModels.Events.CompetitionsViewModel"/> class.
        /// </summary>

        public CompetitionsViewModel()
        {
            Competitions = new ObservableCollection<Competition>
            {

            };
            this.LoadItemsCommand.Execute(null);
        }


        public ObservableCollection<Competition> Competitions { get; }


        private RelayCommand refreshItemsCommand;
        public ICommand RefreshCommand
        {
            get
            {
                return refreshItemsCommand ?? (refreshItemsCommand = new RelayCommand(async () => await ExecuteLoadItemsCommand()));
            }
        }

        private bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged("IsRefreshing");
            }
        }

        /// <summary>
        /// Provides a mechanism to execute the asynchronous load items command.
        /// </summary>
        /// <returns></returns>
        protected override async Task ExecuteLoadItemsCommand()
        {
            IsRefreshing = true;
            await base.ExecuteLoadItemsCommand();
            IsRefreshing = false;
        }

        /// <summary>
        /// Loads the competitions from the backend server.
        /// </summary>
        /// <returns></returns>
        protected override async Task LoadItemsAsync()
        {

            try
            {
                // Make async request to obtain data
                var client = new RestClient(GlobalConstants.EndPointURL);
                var request = new RestRequest
                {
                    Timeout = GlobalConstants.RequestTimeout
                };
                request.Resource = GlobalConstants.CompetitiveEventsEndPointRequestURL;
                UserManager.Current.AddAuthorization(request);

                try
                {

                    DataAvailable = false;

                    var response = await client.ExecuteCachedAPITaskAsync(request, GlobalConstants.MaxCacheCompetitiveEvents, false, true);

                    ErrorMessage = response.ErrorMessage;
                    IsError = !response.IsSuccessful;

                    if (response.IsSuccessful)
                    {
                        Competitions.Clear();

                        var items = JsonConvert.DeserializeObject<List<Competition>>(response.Content) ?? new List<Competition>();

                        foreach (var competition in items)
                        {
                            Competitions.Add(competition);
                        }

                        OnPropertyChanged("Competitions");

                        DataAvailable = true;
                    }
                }
                catch (Exception e)
                {
                    var properties = new Dictionary<string, string> {
                    { "Category", "Competitions" }
                  };
                    Crashes.TrackError(e, properties);

                }
            }
            catch (Exception)
            {
                // An exception occurred
                DataAvailable = false;
            }
        }

    }
}

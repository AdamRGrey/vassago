var apiUrl = '/api/';

function Account(displayName, accountId, protocol){
  this.displayName = displayName;
  this.accountId = accountId;
  this.protocol = protocol;
}
//todo: figure out what the URL actually needs to be, rather than assuming you get a whole-ass server to yourself.
//you selfish fuck... What are you, fox?
//as it stands, you want something like /api/Channels/, trailing slash intentional
function patchModel(model, deprecated_apiUrl)
{
    //structure the model your (dang) self into a nice object
    console.log(model);
    console.log(window.location.pathname);
    var components = window.location.pathname.split('/');
    // if(components[2] !== "Details")
    // {
    //     console.log("wtf are you doing? " + components[2] + " is something other than Details");
    // }
    var type=components[1];
    // var id=components[3];

    console.log("dexter impression: I am now ready to post the following content:");
    console.log(JSON.stringify(model));
    fetch(apiUrl + type + '/', {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(model),
      })
        .then(response => {
          if (!response.ok) {
            throw new Error('Network response was not "ok". which is not ok.');
          }
          return response.json();
        })
        .then(returnedSuccessdata => {
          // perhaps a success callback
          console.log('returnedSuccessdata:', returnedSuccessdata);
        })
        .catch(error => {
          console.error('Error:', error);
        });
}

function deleteModel(model, deprecated_apiUrl)
{
  var components = window.location.pathname.split('/');
  // if(components[2] !== "Details")
  // {
  //     console.log("wtf are you doing? " + components[2] + " is something other than Details");
  // }
  var type=components[1];
  // var id=components[3];
  fetch(apiUrl + type + '/', {
    method: 'DELETE',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(model),
  })
  .then(response => {
    if (!response.ok) {
      throw new Error('Network response was not "ok". which is not ok.');
    }
    return response.json();
  })
  .then(returnedSuccessdata => {
    // perhaps a success callback
    console.log('returnedSuccessdata:', returnedSuccessdata);
  })
  .catch(error => {
    console.error('Error:', error);
  });
}
function linkUAC_Channel(channel_guid)
{
  var components = window.location.pathname.split('/');
  var id=components[3];
  let model={"uac_guid": id,
             "channel_guid": channel_guid};
  fetch(apiUrl + "UAC/LinkChannel/", {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(model),
  })
  .then(response => {
    if (!response.ok) {
      throw new Error('Network response was not "ok". which is not ok.');
    }
    return response.json();
  })
  .then(returnedSuccessdata => {
    // perhaps a success callback
    console.log('returnedSuccessdata:', returnedSuccessdata);
  })
  .catch(error => {
    console.error('Error:', error);
  });
}
function linkUAC_User(user_guid)
{
  var components = window.location.pathname.split('/');
  var id=components[3];
  let model={"uac_guid": id,
             "user_guid": user_guid};
  fetch(apiUrl + "UAC/LinkUser/", {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(model),
  })
  .then(response => {
    if (!response.ok) {
      throw new Error('Network response was not "ok". which is not ok.');
    }
    return response.json();
  })
  .then(returnedSuccessdata => {
    // perhaps a success callback
    console.log('returnedSuccessdata:', returnedSuccessdata);
  })
  .catch(error => {
    console.error('Error:', error);
  });
}
function linkUAC_Account(account_guid)
{
  var components = window.location.pathname.split('/');
  var id=components[3];
  let model={"uac_guid": id,
             "account_guid": account_guid};
  fetch(apiUrl + "UAC/LinkAccount/", {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(model),
  })
  .then(response => {
    if (!response.ok) {
      throw new Error('Network response was not "ok". which is not ok.');
    }
    return response.json();
  })
  .then(returnedSuccessdata => {
    // perhaps a success callback
    console.log('returnedSuccessdata:', returnedSuccessdata);
  })
  .catch(error => {
    console.error('Error:', error);
  });
}

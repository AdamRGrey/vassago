var apiUrl = '/api/';

function Account(displayName, accountId, protocol){
  this.displayName = displayName;
  this.accountId = accountId;
  this.protocol = protocol;
}
//todo: figure out what the URL actually needs to be, rather than assuming you get a whole-ass server to yourself.
//you selfish fuck... What are you, fox?
//as it stands, you want something like /api/Channels/, trailing slash intentional
function patchModel(model, callback)
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

    console.log(JSON.stringify(model));
    fetch(apiUrl + 'Rememberer/' + type + '/', {
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

function deleteModel(id, callback)
{
  var components = window.location.pathname.split('/');
  var type=components[1];
  let result = null;
  var id=components[3];
  fetch(apiUrl + 'Rememberer/' + type + '/' + id, {
    method: 'DELETE',
    headers: {
      'Content-Type': 'application/json',
    }
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
    if(callback !== null) { callback(); }
  })
  .catch(error => {
    console.error('Error:', error);
  });
}
function linkUAC_Channel(channel_guid, callback)
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
    if(callback !== null) { callback(); }
  })
  .catch(error => {
    console.error('Error:', error);
  });
  }
function linkUAC_User(user_guid, callback)
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
    if(callback !== null) { callback(); }
  })
  .catch(error => {
    console.error('Error:', error);
  });
  }
function linkUAC_Account(account_guid, callback)
{
  var reuslt = null;
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
    if(callback !== null) { callback(); }
  })
  .catch(error => {
    console.error('Error:', error);
  });
  }
function unlinkUAC_User(user_guid, callback)
{
  var components = window.location.pathname.split('/');
  var id=components[3];
  let model={"uac_guid": id,
             "user_guid": user_guid};
  fetch(apiUrl + "UAC/UnlinkUser/", {
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
    if(callback !== null) { callback(); }
  })
  .catch(error => {
    console.error('Error:', error);
  });
  }
function unlinkUAC_Account(account_guid, callback)
{
  var components = window.location.pathname.split('/');
  var id=components[3];
  let model={"uac_guid": id,
             "account_guid": account_guid};
  fetch(apiUrl + "UAC/UnlinkAccount/", {
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
    if(callback !== null) { callback(); }
  })
  .catch(error => {
    console.error('Error:', error);
  });
}
function unlinkUAC_Channel(user_guid, callback)
{
  var components = window.location.pathname.split('/');
  var id=components[3];
  let model={"uac_guid": id,
             "channel_guid": user_guid};
  fetch(apiUrl + "UAC/UnlinkChannel/", {
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
    if(callback !== null) { callback(); }
  })
  .catch(error => {
    console.error('Error:', error);
  });
}
//give me account, we'll tear it off from user.
function unlinkAccountUser(callback)
{
  var components = window.location.pathname.split('/');
  var id=components[3];
  let model={"acc_guid": id};
  fetch(apiUrl + "Accounts/UnlinkUser/", {
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
    if(callback !== null) { callback(); }
  })
  .catch(error => {
    console.error('Error:', error);
  });
}

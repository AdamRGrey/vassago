// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function testfunct(caller){
    console.log("[gibberish]");
    console.log(caller);
}
//todo: figure out what the URL actually needs to be, rather than assuming you get a whole-ass server to yourself.
//you selfish fuck... What are you, fox?
//as it stands, you want something like /api/Channels/, trailing slash intentional
function patchModel(model, apiUrl)
{
    //structure the model your (dang) self into a nice object
    console.log(model);
    //i know the page url.
    console.log(window.location.pathname);
    var components = window.location.pathname.split('/');
    if(components[2] !== "Details")
    {
        console.log("wtf are you doing? " + components[2] + " is something other than Details");
        //add different endpoings here, if you like
    }
    var type=components[1];
    var id=components[3];

    console.log("dexter impression: I am now ready to post the following content:");
    console.log(JSON.stringify(model));
    fetch(apiUrl, {
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
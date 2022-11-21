// clientID: 1d9fefd0-6c19-4e4b-a2fb-8b730a8080eb
// tenant: jumpybugsoutlook.onmicrosoft.com
// https://graph.windows.net

var angularApp = angular.module('angularApp', ['AdalAngular']);

angularApp.config(
    ['$httpProvider', 'adalAuthenticationServiceProvider', '$locationProvider',
    function ($httpProvider, adalProvider, locationProvider) {
        locationProvider.hashPrefix('');
        var endpoints = {
            'https://graph.microsoft.com': 'https://graph.microsoft.com'
        };

        adalProvider.init({
            instance: 'https://login.microsoftonline.com/',
            tenant: 'jumpybugsoutlook.onmicrosoft.com',
            clientId: '1d9fefd0-6c19-4e4b-a2fb-8b730a8080eb',
            endpoints: endpoints
        }, $httpProvider);
    }]);

var angularController = angularApp.controller('angularController', [
    '$scope', '$http', 'adalAuthenticationService',
    function ($scope, $http, adalService) {
        $scope.runCommand = function () {
            $http.get('https://graph.microsoft.com/v1.0/me')
                .then(function (returnValue) {
                $scope.returnValue = returnValue;
            });
        }

        $scope.login = function () {
            adalService.login();
        }

        $scope.logout = function () {
            adalService.logOut();
        }
    }]);
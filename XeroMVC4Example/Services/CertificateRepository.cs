﻿using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DevDefined.OAuth.Consumer;

namespace XeroMVC4Example.Services
{
	public static class CertificateRepository
	{
		public static X509Certificate2 GetOAuthSigningCertificate()
		{
			var oauthCertificateFindType = ConfigurationManager.AppSettings["XeroApiOAuthCertificateFindType"];
			var oauthCertificateFindValue = ConfigurationManager.AppSettings["XeroApiOAuthCertificateFindValue"];

			if (string.IsNullOrEmpty(oauthCertificateFindType) || string.IsNullOrEmpty(oauthCertificateFindValue))
			{
				return null;
			}

			var x509FindType = (X509FindType)Enum.Parse(typeof(X509FindType), oauthCertificateFindType);

			// Search the LocalMachine certificate store for matching X509 certificates.
			var certStore = new X509Store("My", StoreLocation.LocalMachine);
			certStore.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
			var certificateCollection = certStore.Certificates.Find(x509FindType, oauthCertificateFindValue, false);
			certStore.Close();

			if (certificateCollection.Count == 0)
			{
				throw new ApplicationException(string.Format("An OAuth certificate matching the X509FindType '{0}' and Value '{1}' cannot be found in the local certificate store.", oauthCertificateFindType, oauthCertificateFindValue));
			}

			return certificateCollection[0];
		}

		/// <summary>
		/// Gets the OAuth signing certificate from the local certificate store, if specified in app.config.
		/// </summary>
		/// <returns></returns>
		public static AsymmetricAlgorithm GetOAuthSigningCertificatePrivateKey()
		{
			var oauthSigningCertificate = GetOAuthSigningCertificate();

			if (oauthSigningCertificate == null)
			{
				return null;
			}

			if (!oauthSigningCertificate.HasPrivateKey)
			{
				throw new ApplicationException("The specified OAuth Certificate find details matched a certificate, but there is not private key stored in the certificate");
			}

			return oauthSigningCertificate.PrivateKey;
		}

		public static X509Certificate2 GetClientSslCertificate()
		{
			var clientSslCertificateFactory = GetClientSslCertificateFactory();
			return clientSslCertificateFactory.CreateCertificate();
		}

		/// <summary>
		/// Return a CertificateFactory that can read the Client SSL certificate from the local machine certificate store
		/// </summary>
		/// <returns></returns>
		public static ICertificateFactory GetClientSslCertificateFactory()
		{
			var oauthCertificateFindType = ConfigurationManager.AppSettings["XeroApiSslCertificateFindType"];
			var oauthCertificateFindValue = ConfigurationManager.AppSettings["XeroApiSslCertificateFindValue"];

			if (string.IsNullOrEmpty(oauthCertificateFindType) || string.IsNullOrEmpty(oauthCertificateFindValue))
			{
				return new NullCertificateFactory();
			}

			var x509FindType = (X509FindType)Enum.Parse(typeof(X509FindType), oauthCertificateFindType);
			var certificateFactory = new LocalMachineCertificateFactory(oauthCertificateFindValue, x509FindType);

			if (certificateFactory.CreateCertificate() == null)
			{
				throw new ApplicationException(string.Format("A client SSL certificate matching the X509FindType '{0}' and value '{1}' cannot be found in the local certificate store.", oauthCertificateFindType, oauthCertificateFindValue));
			}

			return certificateFactory;
		}

	}
}
/**
 * \file libiorfidwrapper.h
 * \author Julien KAUFFMANN <julien.kauffmann@islog.eu>
 * \brief LibBIORFIDWrapper public DLL interface.
 */

#ifndef LIBIORFIDWRAPPER_H
#define LIBIORFIDWRAPPER_H

#ifdef LIBBIORFIDWRAPPER_EXPORTS
#define LIBBIORFIDWRAPPER_API __declspec(dllexport)
#else
#define LIBBIORFIDWRAPPER_API __declspec(dllimport)
#endif

#ifdef UNICODE
#define BioRFIDLoadModule BioRFIDLoadModuleW
#else
#define BioRFIDLoadModule BioRFIDLoadModuleA
#endif

#define MODULE_HANDLE int
#define NULL_MODULE_HANDLE 0

#include "templateitem.h"

/**
 * \brief Load and set the current module.
 * \param moduleFileName The module filename to use, in ascii. If moduleFileName is NULL, the current module will be unloaded.
 * \return A positive handle number on success, zero on failure.
 * \see BioRFIDUnloadModule
 */
extern "C" int LIBBIORFIDWRAPPER_API BioRFIDLoadModuleA(const char* moduleFileName);

/**
 * \brief Load and set the current module.
 * \param moduleFileName The module filename to use, in unicode. If moduleFileName is NULL, the current module will be unloaded.
 * \return A positive handle number on success, zero on failure.
 * \see BioRFIDUnloadModule
 */
extern "C" int LIBBIORFIDWRAPPER_API BioRFIDLoadModuleW(const wchar_t* moduleFileName);

/**
 * \brief Unload a module.
 * \param handle The handle as returned by BioRFIDLoadModule().
 * \see BioRFIDLoadModule
 */
extern "C" void LIBBIORFIDWRAPPER_API BioRFIDUnloadModule(int handle);

/**
 * \brief Capture a template from the selected module.
 * \param handle The handle as returned by BioRFIDLoadModule().
 * \param buf The address of a pointer to the captured template. *buf will be initialized on success. *buf must be freed using BioRFIDRelease().
 * \param buflen A pointer to the new length of *buf.
 * \param timeout The timeout value, in millisecond for the capture.
 * \return A positive number on success, zero on failure and a negative number on timeout.
 * \see BioRFIDRelease
 *
 * If the call fails, buf and buflen are unchanged (no initialization is done). Thus, you only need to call Release() on a successful return.
 */
extern "C" int LIBBIORFIDWRAPPER_API BioRFIDCapture(int handle, void** buf, size_t* buflen, int timeout);

/**
 * \brief Release a capture.
 * \param handle The handle as returned by BioRFIDLoadModule
 * \param buf The capture to release. *buf is undefined after the call.
 * \see BioRFIDCapture
 */
extern "C" void LIBBIORFIDWRAPPER_API BioRFIDRelease(int handle, void* buf);

/**
 * \brief Validate a supplied template. Used for authentication.
 * \param handle The handle as returned by BioRFIDLoadModule().
 * \param buf The template.
 * \param buflen The length of buf.
 * \param timeout The timeout to validation.
 * \return A positive number on success, zero on failure and a negative number on timeout.
 * \see BioRFIDCapture
 */
extern "C" int LIBBIORFIDWRAPPER_API BioRFIDValidate(int handle, const void* buf, size_t buflen, int timeout);

/**
 * \brief Allocate a new template item, eventually adding it to an existing template item linked list.
 * \brief int handle The handle as returned by BioRFIDLoadModule().
 * \brief templates The template items to add the new template item to. If template is NULL, a new template item linked list is created.
 * \brief buf The template to copy data from.
 * \brief buflen The length of buf.
 * \return NULL on failure. Otherwise, if template is NULL, returns a pointer to a new template item linked list. If template is not NULL, returns template.
 * \warning The function copies the content of the memory pointed by buf into an internally allocated block of memory. So, you may release buf at any time after the call.
 * \warning You may only release the returned value when templates is NULL unless you want to face some serious double-free issues :)
 * \see BioRFIDReleaseTemplates
 */
extern "C" PTemplateItem LIBBIORFIDWRAPPER_API BioRFIDAddTemplate(int handle, PTemplateItem templates, const void* buf, size_t buflen);

/**
 * \brief Frees a template item linked list.
 * \brief int handle The handle as returned by BioRFIDLoadModule().
 * \brief templates The template items to delete.
 */
extern "C" void LIBBIORFIDWRAPPER_API BioRFIDReleaseTemplates(int handle, PTemplateItem templates);

/**
 * \brief Find a matching template among several supplied templates. Used for identification.
 * \param handle The handle as returned by BioRFIDLoadModule.
 * \param templates The template items to search into.
 * \param timeout The timeout to identification.
 * \return A pointer to the matching template item, or NULL if no matching template is found.
 * \warning You may NEVER call BioRFIDReleaseTemplates on the returned value unless you want to face some serious double-free or memory leak issues :)
 */
extern "C" const PTemplateItem LIBBIORFIDWRAPPER_API BioRFIDFind(int handle, const PTemplateItem templates, int timeout);

#endif /* LIBIORFIDWRAPPER_H */

